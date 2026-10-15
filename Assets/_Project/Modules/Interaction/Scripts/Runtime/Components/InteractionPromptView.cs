using TMPro;
using Trivias.Shared.Events;
using UnityEngine;

namespace Trivias.Interaction.Components
{
    /// <summary>Aviso de interacción en pantalla ("E: coger fajo"). Se actualiza solo por eventos.</summary>
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private StringEventChannel _promptChannel;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private GameObject _panel;

        private void OnEnable()
        {
            _promptChannel.Register(Show);
            Show(string.Empty);
        }

        private void OnDisable() => _promptChannel.Unregister(Show);

        private void Show(string prompt)
        {
            var visible = !string.IsNullOrEmpty(prompt);
            _panel.SetActive(visible);
            if (visible)
                _label.text = prompt;
        }
    }
}
