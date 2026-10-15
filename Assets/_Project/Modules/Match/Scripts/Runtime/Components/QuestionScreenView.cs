using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Match.Components
{
    /// <summary>Pantalla grande del plató: número de pregunta o resultado final.</summary>
    public sealed class QuestionScreenView : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private TextMeshPro _title;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => _runtime.Changed -= Refresh;

        private void Refresh()
        {
            var session = _runtime.Session;
            var text = session == null ? "TRIVIAS" : session.Phase switch
            {
                MatchPhase.Lobby => "¿PREPARADOS?",
                MatchPhase.Victory => "¡VICTORIA!",
                MatchPhase.Defeat => "DERROTA",
                _ => $"PREGUNTA {session.QuestionNumber} / {session.TotalQuestions}",
            };

            if (_title.text != text)
                _title.text = text;
        }
    }
}
