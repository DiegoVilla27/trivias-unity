using Trivias.Shared.Characters;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Presentation;
using Trivias.Shared.Profile;
using Trivias.Shop.Data;
using Trivias.Shop.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trivias.Shop.Components
{
    /// <summary>
    /// Tienda (GDD 11), como los bocetos 04 y 05: pestañas Personajes y Accesorios, tarjetas con su foto y su estado, y a
    /// la derecha la vista 3D del artículo elegido (el personaje, o tu personaje con el complemento puesto), su precio o
    /// estado y el botón que corresponda. Se abre desde la sala de espera y se cierra sola al empezar la partida. Se
    /// redibuja solo cuando cambia algo (perfil, retratos o selección).
    /// </summary>
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private CosmeticCatalog _catalog;
        [SerializeField] private CharacterCatalog _characters;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private UiBackdrop _backdrop;
        [SerializeField] private VoidEventChannel _openRequested;
        [SerializeField] private PlayerControlGate _controlGate;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _tokens;

        [Header("Pestañas")]
        [SerializeField] private Button _charactersTab;
        [SerializeField] private Button _accessoriesTab;
        [SerializeField] private TextMeshProUGUI _charactersTabLabel;
        [SerializeField] private TextMeshProUGUI _accessoriesTabLabel;
        [SerializeField] private GameObject _charactersGrid;
        [SerializeField] private GameObject _accessoriesGrid;
        [SerializeField] private ShopCard[] _characterCards;
        [SerializeField] private ShopCard[] _accessoryCards;

        [Header("Vista del artículo")]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _badge;
        [SerializeField] private Image _badgeIcon;
        [SerializeField] private TextMeshProUGUI _badgeText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionLabel;
        [SerializeField] private Button _closeButton;

        [Header("Aspecto")]
        [SerializeField] private Sprite _tabSelected;
        [SerializeField] private Sprite _tabIdle;
        [SerializeField] private Sprite _card;
        [SerializeField] private Sprite _cardSelected;
        [SerializeField] private Sprite _badgeDark;
        [SerializeField] private Sprite _badgeGreen;
        [SerializeField] private Sprite _badgeGold;
        [SerializeField] private Sprite _checkIcon;
        [SerializeField] private Sprite _tokenIcon;
        [SerializeField] private Color _darkText = new(0.06f, 0.09f, 0.19f);
        [SerializeField] private Color _lightText = new(0.96f, 0.94f, 0.88f);

        private UnityAction[] _characterActions;
        private UnityAction[] _accessoryActions;
        private int _characterIndex;
        private int _accessoryIndex;

        public bool IsOpen => _panel.activeSelf;
        public bool IsAccessoriesTab { get; private set; }
        public string SelectedId => IsAccessoriesTab ? _catalog.Items[_accessoryIndex].Id : _characters.Ids[_characterIndex];
        public string ActionText => _actionLabel.text;
        public bool CanAct => _actionButton.interactable;

        private void Awake()
        {
            _characterActions = new UnityAction[_characterCards.Length];
            for (var i = 0; i < _characterCards.Length; i++)
            {
                var index = i;
                _characterActions[i] = () => SelectCharacter(index);
            }
            _accessoryActions = new UnityAction[_accessoryCards.Length];
            for (var i = 0; i < _accessoryCards.Length; i++)
            {
                var index = i;
                _accessoryActions[i] = () => SelectAccessory(index);
            }
        }

        private void OnEnable()
        {
            _openRequested.Register(Open);
            _controlGate.Changed += OnControlGateChanged;
            _profile.Changed += Refresh;
            _preview.PortraitsChanged += Refresh;
            _charactersTab.onClick.AddListener(ShowCharacters);
            _accessoriesTab.onClick.AddListener(ShowAccessories);
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(Next);
            _actionButton.onClick.AddListener(PressAction);
            _closeButton.onClick.AddListener(Close);
            for (var i = 0; i < _characterCards.Length; i++)
                _characterCards[i].Button.onClick.AddListener(_characterActions[i]);
            for (var i = 0; i < _accessoryCards.Length; i++)
                _accessoryCards[i].Button.onClick.AddListener(_accessoryActions[i]);
        }

        private void OnDisable()
        {
            _openRequested.Unregister(Open);
            _controlGate.Changed -= OnControlGateChanged;
            _profile.Changed -= Refresh;
            _preview.PortraitsChanged -= Refresh;
            _charactersTab.onClick.RemoveListener(ShowCharacters);
            _accessoriesTab.onClick.RemoveListener(ShowAccessories);
            _previousButton.onClick.RemoveListener(Previous);
            _nextButton.onClick.RemoveListener(Next);
            _actionButton.onClick.RemoveListener(PressAction);
            _closeButton.onClick.RemoveListener(Close);
            for (var i = 0; i < _characterCards.Length; i++)
                _characterCards[i].Button.onClick.RemoveListener(_characterActions[i]);
            for (var i = 0; i < _accessoryCards.Length; i++)
                _accessoryCards[i].Button.onClick.RemoveListener(_accessoryActions[i]);
            _backdrop.Cover(this, false);
        }

        /// <summary>Se abre en Personajes, con el personaje que llevas seleccionado.</summary>
        public void Open()
        {
            _panel.SetActive(true);
            _backdrop.Cover(this, true); // Tapa la sala de espera (y deja de pedir su vista).
            foreach (var item in _catalog.Items)
                _preview.RequestAccessoryPortrait(item.Id, item.Prefab, item.Offset, item.Slot);
            var current = Current();
            _characterIndex = Mathf.Max(0, IndexOf(current));
            IsAccessoriesTab = false;
            Refresh();
        }

        public void Close()
        {
            _preview.Wear(string.Empty, null, Vector3.zero);
            _panel.SetActive(false);
            _backdrop.Cover(this, false); // La sala de espera vuelve y pide otra vez tu personaje.
        }

        public void ShowCharacters()
        {
            IsAccessoriesTab = false;
            Refresh();
        }

        public void ShowAccessories()
        {
            IsAccessoriesTab = true;
            Refresh();
        }

        /// <summary>Selecciona un artículo por su id (en su pestaña).</summary>
        public void Select(string id)
        {
            var character = IndexOf(id);
            if (character >= 0)
            {
                IsAccessoriesTab = false;
                _characterIndex = character;
                Refresh();
                return;
            }
            for (var i = 0; i < _catalog.Items.Count; i++)
            {
                if (_catalog.Items[i].Id != id)
                    continue;
                IsAccessoriesTab = true;
                _accessoryIndex = i;
                Refresh();
                return;
            }
        }

        private void SelectCharacter(int index)
        {
            _characterIndex = index;
            Refresh();
        }

        private void SelectAccessory(int index)
        {
            _accessoryIndex = index;
            Refresh();
        }

        private void Previous() => Move(-1);

        private void Next() => Move(1);

        private void Move(int direction)
        {
            if (IsAccessoriesTab)
                _accessoryIndex = ShopRules.Step(_accessoryIndex, direction, _catalog.Items.Count);
            else
                _characterIndex = ShopRules.Step(_characterIndex, direction, _characters.Ids.Count);
            Refresh();
        }

        /// <summary>El botón principal: comprar, elegir, equipar o quitar, según el artículo.</summary>
        public void PressAction()
        {
            var data = _profile.Data;
            if (!IsAccessoriesTab)
            {
                var id = _characters.Ids[_characterIndex];
                if (!_characters.IsFree(id) && !ProfileRules.Owns(data, id))
                    _profile.TryBuy(id, _characters.Price);
                _profile.TryChooseCharacter(_characters, id);
                return;
            }

            var item = _catalog.Items[_accessoryIndex];
            if (!ProfileRules.Owns(data, item.Id))
            {
                if (_profile.TryBuy(item.Id, item.Price))
                    _profile.TryEquip(item.Slot, item.Id);
            }
            else if (data.Equipped(item.Slot) == item.Id)
                _profile.Unequip(item.Slot);
            else
                _profile.TryEquip(item.Slot, item.Id);
        }

        private void OnControlGateChanged(bool canMove)
        {
            // Al empezar la partida se cierra: la tienda es del vestíbulo.
            if (canMove && IsOpen)
                Close();
        }

        private string Current() => CharacterRules.Current(_profile.CharacterId, _characters.Ids, _characters.FreeIds, _profile.Data.owned);

        private int IndexOf(string characterId)
        {
            var ids = _characters.Ids;
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == characterId)
                    return i;
            }
            return -1;
        }

        private void Refresh()
        {
            if (!IsOpen)
                return;

            var data = _profile.Data;
            _tokens.text = ShopRules.Tokens(data.tokens);
            _charactersTab.image.sprite = IsAccessoriesTab ? _tabIdle : _tabSelected;
            _accessoriesTab.image.sprite = IsAccessoriesTab ? _tabSelected : _tabIdle;
            _charactersTabLabel.color = IsAccessoriesTab ? _lightText : _darkText;
            _accessoriesTabLabel.color = IsAccessoriesTab ? _darkText : _lightText;
            _charactersGrid.SetActive(!IsAccessoriesTab);
            _accessoriesGrid.SetActive(IsAccessoriesTab);

            var current = Current();
            var ids = _characters.Ids;
            for (var i = 0; i < _characterCards.Length; i++)
            {
                if (i >= ids.Count)
                {
                    _characterCards[i].gameObject.SetActive(false);
                    continue;
                }
                var character = _characters.Find(ids[i]);
                var offer = CharacterOffer(ids[i], current);
                var locked = offer.State == OfferState.Locked;
                _characterCards[i].Show(character.DisplayName, _preview.Body(ids[i]), i == _characterIndex && !IsAccessoriesTab,
                    i == _characterIndex ? _cardSelected : _card, _badgeDark, locked ? _tokenIcon : _checkIcon, offer.Badge,
                    _lightText, locked);
            }

            for (var i = 0; i < _accessoryCards.Length; i++)
            {
                if (i >= _catalog.Items.Count)
                {
                    _accessoryCards[i].gameObject.SetActive(false);
                    continue;
                }
                var item = _catalog.Items[i];
                var offer = AccessoryOffer(item);
                var locked = offer.State == OfferState.Locked;
                _accessoryCards[i].Show(item.DisplayName, _preview.AccessoryPortrait(item.Id), i == _accessoryIndex,
                    i == _accessoryIndex ? _cardSelected : _card, locked ? _badgeGold : _badgeGreen, locked ? _tokenIcon : _checkIcon,
                    offer.Badge, locked ? _darkText : _lightText, false);
            }

            // A la derecha: el artículo seleccionado.
            ShopOffer selected;
            if (IsAccessoriesTab)
            {
                var item = _catalog.Items[_accessoryIndex];
                selected = AccessoryOffer(item);
                _name.text = item.DisplayName;
                _preview.Request(current);
                _preview.Wear(item.Id, item.Prefab, item.Offset, item.Slot);
            }
            else
            {
                var id = ids[_characterIndex];
                selected = CharacterOffer(id, current);
                _name.text = _characters.Find(id).DisplayName;
                _preview.Wear(string.Empty, null, Vector3.zero);
                _preview.Request(id);
            }
            var lockedSelection = selected.State == OfferState.Locked;
            _badge.sprite = lockedSelection ? _badgeGold : selected.State == OfferState.Free ? _badgeDark : _badgeGreen;
            _badgeIcon.sprite = lockedSelection ? _tokenIcon : _checkIcon;
            _badgeText.text = selected.Badge;
            _badgeText.color = lockedSelection ? _darkText : _lightText;
            _actionLabel.text = selected.Action;
            _actionButton.interactable = selected.CanAct;
        }

        private ShopOffer CharacterOffer(string id, string current) =>
            ShopRules.Character(_characters.IsFree(id), ProfileRules.Owns(_profile.Data, id), id == current, _characters.Price,
                _profile.Data.tokens);

        private ShopOffer AccessoryOffer(CosmeticItem item)
        {
            var data = _profile.Data;
            var owned = ProfileRules.Owns(data, item.Id);
            return ShopRules.Accessory(owned, owned && data.Equipped(item.Slot) == item.Id, item.Price, data.tokens);
        }
    }
}
