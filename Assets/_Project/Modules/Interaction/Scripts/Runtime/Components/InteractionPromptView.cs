using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Events;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Interaction.Components
{
    /// <summary>
    /// Aviso de interacción en pantalla, abajo en el centro: cada acción con su tecla dibujada («[E] Coger fajo»).
    /// Se actualiza solo cuando cambia el aviso; las piezas ya están creadas (una por acción posible).
    /// </summary>
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private StringEventChannel _promptChannel;
        [SerializeField] private GameObject _panel;
        [Tooltip("Una pieza por acción (tecla y texto), de izquierda a derecha.")]
        [SerializeField] private GameObject[] _slots;
        [SerializeField] private GameObject[] _keys;
        [SerializeField] private TMP_Text[] _keyLabels;
        [SerializeField] private TMP_Text[] _actions;

        private readonly List<(string Key, string Action)> _parts = new();

        /// <summary>Lo que se lee ahora («E Coger fajo · Q Coger 5»), o vacío.</summary>
        public string Shown { get; private set; } = string.Empty;

        private void OnEnable()
        {
            _promptChannel.Register(Show);
            Show(string.Empty);
        }

        private void OnDisable() => _promptChannel.Unregister(Show);

        private void Show(string prompt)
        {
            PromptText.Split(prompt, _parts);
            _panel.SetActive(_parts.Count > 0);
            for (var i = 0; i < _slots.Length; i++)
            {
                var used = i < _parts.Count;
                _slots[i].SetActive(used);
                if (!used)
                    continue;
                var (key, action) = _parts[i];
                _keys[i].SetActive(key.Length > 0);
                _keyLabels[i].text = key;
                _actions[i].text = action;
            }
            Shown = _parts.Count > 0 ? PromptText.Plain(prompt) : string.Empty;
        }
    }
}
