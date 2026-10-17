using Trivias.Shared.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trivias.Menu.Components
{
    /// <summary>Aplica el fondo de los menús (<see cref="UiBackdrop"/>): desenfoca el plató y lo oscurece un poco.</summary>
    public sealed class MenuBackdropView : MonoBehaviour
    {
        [SerializeField] private UiBackdrop _backdrop;
        [SerializeField] private GameObject _dim;
        [SerializeField] private Volume _blur;

        public bool IsShown => _dim.activeSelf;

        private void OnEnable()
        {
            _backdrop.Changed += Apply;
            Apply();
        }

        private void OnDisable() => _backdrop.Changed -= Apply;

        private void Apply()
        {
            _dim.SetActive(_backdrop.IsShown);
            _blur.weight = _backdrop.IsShown ? 1f : 0f;
        }
    }
}
