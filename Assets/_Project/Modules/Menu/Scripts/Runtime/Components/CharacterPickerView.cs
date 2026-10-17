using Trivias.Shared.Characters;
using Trivias.Shared.Profile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Elegir personaje en el menú principal, en su panel a la derecha: el personaje en 3D (lo pinta el expositor del
    /// plató), flechas para pasar por los que tiene el jugador (los gratuitos y los comprados) y su nombre. Se guarda en
    /// el perfil al momento; quien no elige lleva el de por defecto. Los de pago se compran en la tienda.
    /// </summary>
    public sealed class CharacterPickerView : MonoBehaviour
    {
        [SerializeField] private CharacterCatalog _catalog;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private RawImage _image;
        [SerializeField] private TextMeshProUGUI _name;
        [Tooltip("Pastilla bajo el nombre: «Gratis · Elegido» o «Comprado · Elegido».")]
        [SerializeField] private TextMeshProUGUI _status;
        [Tooltip("«4 más en la tienda».")]
        [SerializeField] private TextMeshProUGUI _hint;

        /// <summary>Personaje que se ve elegido.</summary>
        public string ShownId { get; private set; } = string.Empty;

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(Next);
            _profile.Changed += Refresh;
            _preview.TextureChanged += ShowTexture;
            ShowTexture();
            Refresh();
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(Previous);
            _nextButton.onClick.RemoveListener(Next);
            _profile.Changed -= Refresh;
            _preview.TextureChanged -= ShowTexture;
            _preview.Request(string.Empty); // Sin menú, el expositor se apaga.
        }

        public void Previous() => Step(-1);

        public void Next() => Step(1);

        private void Step(int direction)
        {
            var next = CharacterRules.Step(Current(), direction, _catalog.Ids, _catalog.FreeIds, _profile.Data.owned);
            if (!_profile.TryChooseCharacter(_catalog, next))
                Refresh();
        }

        private string Current() =>
            CharacterRules.Current(_profile.CharacterId, _catalog.Ids, _catalog.FreeIds, _profile.Data.owned);

        private void ShowTexture()
        {
            _image.texture = _preview.Texture;
            _image.enabled = _preview.Texture != null;
        }

        private void Refresh()
        {
            ShownId = Current();
            var character = _catalog.Find(ShownId);
            _preview.Request(ShownId);
            _name.text = character != null ? character.DisplayName : string.Empty;

            var locked = 0;
            foreach (var id in _catalog.Ids)
            {
                if (!CharacterRules.IsUnlocked(id, _catalog.FreeIds, _profile.Data.owned))
                    locked++;
            }
            _status.text = $"{(_catalog.IsFree(ShownId) ? "Gratis" : "Comprado")} · Elegido";
            _hint.text = locked > 0 ? $"{locked} más en la tienda" : "Los tienes todos";

            var canChange = CharacterRules.Step(ShownId, 1, _catalog.Ids, _catalog.FreeIds, _profile.Data.owned) != ShownId;
            _previousButton.interactable = canChange;
            _nextButton.interactable = canChange;
        }
    }
}
