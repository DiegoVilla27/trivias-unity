using TMPro;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>Nombre flotante sobre el concursante. La orientación hacia la cámara la hace Billboard (Shared).</summary>
    public sealed class PlayerNameTag : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _label;

        public void SetName(string displayName) => _label.text = displayName;
    }
}
