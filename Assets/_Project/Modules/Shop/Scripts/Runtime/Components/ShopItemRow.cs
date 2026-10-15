using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shop.Components
{
    /// <summary>Fila de la tienda (creada en tiempo de edición, una por cosmético del catálogo).</summary>
    public sealed class ShopItemRow : MonoBehaviour
    {
        [SerializeField] private int _itemIndex;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonLabel;

        public int ItemIndex => _itemIndex;
        public TextMeshProUGUI Label => _label;
        public Button Button => _button;
        public TextMeshProUGUI ButtonLabel => _buttonLabel;
    }
}
