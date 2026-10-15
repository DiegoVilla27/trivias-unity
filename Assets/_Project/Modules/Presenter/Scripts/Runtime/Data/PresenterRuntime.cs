using System;
using UnityEngine;

namespace Trivias.Presenter.Data
{
    /// <summary>Frase que está diciendo el presentador ahora mismo; la muestran el bocadillo y el subtítulo.</summary>
    [CreateAssetMenu(menuName = "Trivias/Presenter/Presenter Runtime", fileName = "PresenterRuntime")]
    public sealed class PresenterRuntime : ScriptableObject
    {
        /// <summary>Nueva frase (ya con las variables rellenadas) o vacío para callar.</summary>
        public event Action<string> Spoke;

        public string CurrentLine { get; private set; } = string.Empty;

        public void Say(string line)
        {
            CurrentLine = line ?? string.Empty;
            Spoke?.Invoke(CurrentLine);
        }

        private void OnEnable() => CurrentLine = string.Empty;
    }
}
