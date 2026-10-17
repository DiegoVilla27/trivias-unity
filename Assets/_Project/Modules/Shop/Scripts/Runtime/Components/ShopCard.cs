using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shop.Components
{
    /// <summary>Tarjeta de la tienda: foto, nombre y pastilla de estado (precio con candado, «Gratis», «Adquirido»…).</summary>
    public sealed class ShopCard : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private RawImage _portrait;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _badge;
        [SerializeField] private Image _badgeIcon;
        [SerializeField] private TextMeshProUGUI _badgeText;
        [SerializeField] private Image _lock;

        public Button Button => _button;
        public string BadgeText => _badgeText.text;
        public bool IsLocked => _lock.enabled;

        public void Show(string displayName, Texture portrait, bool selected, Sprite background, Sprite badge, Sprite badgeIcon,
            string badgeText, Color badgeTextColor, bool locked)
        {
            _name.text = displayName;
            _portrait.texture = portrait;
            _portrait.enabled = portrait != null;
            _background.sprite = background;
            _badge.sprite = badge;
            _badgeIcon.sprite = badgeIcon;
            _badgeIcon.enabled = badgeIcon != null;
            _badgeText.text = badgeText;
            _badgeText.color = badgeTextColor;
            _lock.enabled = locked;
        }
    }
}
