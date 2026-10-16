using System;
using Trivias.Presenter.Logic;
using UnityEngine;

namespace Trivias.Presenter.Data
{
    /// <summary>Frase que está diciendo el presentador ahora mismo, y con qué ánimo; la muestra su viñeta.</summary>
    [CreateAssetMenu(menuName = "Trivias/Presenter/Presenter Runtime", fileName = "PresenterRuntime")]
    public sealed class PresenterRuntime : ScriptableObject
    {
        /// <summary>Nueva frase (ya con las variables rellenadas) o vacío para callar.</summary>
        public event Action<string> Spoke;

        public string CurrentLine { get; private set; } = string.Empty;
        public PresenterMood Mood { get; private set; }

        public void Say(string line) => Say(line, PresenterMood.Neutral);

        public void Say(string line, PresenterMood mood)
        {
            CurrentLine = line ?? string.Empty;
            Mood = mood;
            Spoke?.Invoke(CurrentLine);
        }

        private void OnEnable()
        {
            CurrentLine = string.Empty;
            Mood = PresenterMood.Neutral;
        }
    }
}
