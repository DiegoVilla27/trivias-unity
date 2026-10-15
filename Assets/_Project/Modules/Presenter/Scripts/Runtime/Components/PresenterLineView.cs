using TMPro;
using Trivias.Presenter.Data;
using UnityEngine;

namespace Trivias.Presenter.Components
{
    /// <summary>
    /// Muestra la frase actual del presentador (bocadillo en el plató o subtítulo en pantalla).
    /// Solo reacciona cuando el presentador habla o calla; nunca por frame.
    /// </summary>
    public sealed class PresenterLineView : MonoBehaviour
    {
        [SerializeField] private PresenterRuntime _runtime;
        [SerializeField] private TMP_Text _label;
        [Tooltip("Lo que se oculta cuando el presentador calla (el propio texto o su panel de fondo).")]
        [SerializeField] private GameObject _container;
        [Tooltip("Texto antes de la frase, p. ej. «PRESENTADOR: ».")]
        [SerializeField] private string _prefix = string.Empty;

        private void OnEnable()
        {
            _runtime.Spoke += Show;
            Show(_runtime.CurrentLine);
        }

        private void OnDisable() => _runtime.Spoke -= Show;

        private void Show(string line)
        {
            var visible = !string.IsNullOrEmpty(line);
            _container.SetActive(visible);
            if (visible)
                _label.text = _prefix + line;
        }
    }
}
