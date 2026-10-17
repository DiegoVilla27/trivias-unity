using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Match.Components
{
    /// <summary>
    /// ✓ grande sobre la cabina correcta al revelar (boceto 06 del HUD): el acierto se ve con un símbolo, no solo con
    /// el color de las bombillas. Se enciende y se apaga por eventos de la partida.
    /// </summary>
    public sealed class BoothRevealMark : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _runtime;
        [SerializeField, Range(0, 3)] private int _booth;
        [SerializeField] private GameObject _mark;

        public bool IsShown => _mark.activeSelf;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => _runtime.Changed -= Refresh;

        private void Refresh()
        {
            var session = _runtime.Session;
            var shown = session != null && session.Phase == MatchPhase.Reveal && session.CorrectBooth == _booth;
            if (_mark.activeSelf != shown)
                _mark.SetActive(shown);
        }
    }
}
