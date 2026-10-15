using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Profile;
using Trivias.Shop.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shop.Components
{
    /// <summary>
    /// Tienda del vestíbulo (GDD 11): comprar con fichas y equipar cosméticos. Se abre desde la sala de espera y se
    /// cierra sola al empezar la partida. Se redibuja solo cuando cambia el perfil.
    /// </summary>
    public sealed class ShopView : MonoBehaviour
    {
        private const string GOLD = "#FFD15C";
        private const string MUTED = "#FFFFFF99";

        [SerializeField] private CosmeticCatalog _catalog;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private VoidEventChannel _openRequested;
        [SerializeField] private PlayerControlGate _controlGate;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _tokens;
        [SerializeField] private ShopItemRow[] _rows = new ShopItemRow[0];
        [SerializeField] private Button _closeButton;

        public bool IsOpen => _panel.activeSelf;

        private void OnEnable()
        {
            _openRequested.Register(Open);
            _controlGate.Changed += OnControlGateChanged;
            _profile.Changed += Refresh;
            _closeButton.onClick.AddListener(Close);
            foreach (var row in _rows)
            {
                var index = row.ItemIndex;
                row.Button.onClick.AddListener(() => OnRowClicked(index));
            }
        }

        private void OnDisable()
        {
            _openRequested.Unregister(Open);
            _controlGate.Changed -= OnControlGateChanged;
            _profile.Changed -= Refresh;
            _closeButton.onClick.RemoveListener(Close);
            foreach (var row in _rows)
                row.Button.onClick.RemoveAllListeners();
        }

        public void Open()
        {
            _panel.SetActive(true);
            Refresh();
        }

        public void Close() => _panel.SetActive(false);

        private void OnControlGateChanged(bool canMove)
        {
            // Al empezar la partida se cierra: la tienda es del vestíbulo.
            if (canMove)
                Close();
        }

        /// <summary>Comprar si no lo tiene; si lo tiene, equipar o quitar.</summary>
        private void OnRowClicked(int index)
        {
            var item = _catalog.Items[index];
            var data = _profile.Data;
            if (!ProfileRules.Owns(data, item.Id))
            {
                if (_profile.TryBuy(item.Id, item.Price))
                    _profile.TryEquip(item.Slot, item.Id);
                return;
            }

            if (data.Equipped(item.Slot) == item.Id)
                _profile.Unequip(item.Slot);
            else
                _profile.TryEquip(item.Slot, item.Id);
        }

        private void Refresh()
        {
            if (!IsOpen)
                return;

            var data = _profile.Data;
            _tokens.text = $"<color={GOLD}><b>{data.tokens}</b></color> fichas";

            foreach (var row in _rows)
            {
                var item = _catalog.Items[row.ItemIndex];
                var owned = ProfileRules.Owns(data, item.Id);
                var equipped = owned && data.Equipped(item.Slot) == item.Id;

                row.Label.text = $"<b>{item.DisplayName}</b>  <color={MUTED}><size=80%>{SlotName(item.Slot)}</size></color>";
                row.ButtonLabel.text = !owned ? $"Comprar · {item.Price}" : equipped ? "Quitar" : "Equipar";
                row.Button.interactable = owned || ProfileRules.CanBuy(data, item.Id, item.Price);
            }
        }

        private static string SlotName(CosmeticSlot slot) => slot switch
        {
            CosmeticSlot.Head => "cabeza",
            CosmeticSlot.Face => "cara",
            CosmeticSlot.Neck => "cuello",
            _ => "traje",
        };
    }
}
