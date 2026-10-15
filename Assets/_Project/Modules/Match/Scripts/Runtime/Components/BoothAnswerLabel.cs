using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Match.Components
{
    /// <summary>
    /// Texto de la respuesta sobre una cabina ("A · París"). Al revelar, la correcta se marca en verde
    /// y las demás se apagan. Se actualiza solo cuando cambia la partida.
    /// </summary>
    public sealed class BoothAnswerLabel : MonoBehaviour
    {
        private static readonly Color Neutral = Color.white;
        private static readonly Color Correct = new(0.35f, 1f, 0.45f);
        private static readonly Color Wrong = new(1f, 0.45f, 0.45f, 0.55f);

        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private TextMeshPro _label;
        [SerializeField, Range(0, 3)] private int _booth;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => _runtime.Changed -= Refresh;

        private void Refresh()
        {
            var session = _runtime.Session;
            var question = session?.CurrentQuestion;
            var letter = (char)('A' + _booth);

            if (question == null || MatchRules.IsFinished(session.Phase))
            {
                _label.text = letter.ToString();
                _label.color = Neutral;
                return;
            }

            var text = $"{letter} · {question.Option(session.Order.OriginalAt(_booth))}";
            if (_label.text != text)
                _label.text = text;

            var correct = session.CorrectBooth;
            _label.color = correct < 0 ? Neutral : correct == _booth ? Correct : Wrong;
        }
    }
}
