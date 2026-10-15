using System;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using UnityEngine;

namespace Trivias.Match.Data
{
    /// <summary>
    /// Anclaje en runtime de la partida en curso. HUD, cabinas, pantalla y botón lo consultan a través de este asset,
    /// sin buscarse en la escena. <see cref="Changed"/> se lanza cuando cambia cualquier dato de la partida.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Match/Match Runtime", fileName = "MatchRuntime")]
    public sealed class MatchRuntime : ScriptableObject
    {
        public event Action Changed;

        public IMatchSession Session { get; private set; }

        public void Attach(IMatchSession session)
        {
            Session = session;
            Changed?.Invoke();
        }

        public void Detach(IMatchSession session)
        {
            if (Session != session)
                return;
            Session = null;
            Changed?.Invoke();
        }

        public void NotifyChanged() => Changed?.Invoke();

        private void OnEnable() => Session = null;
    }

    /// <summary>Lo que la interfaz necesita de la partida en red.</summary>
    public interface IMatchSession
    {
        MatchPhase Phase { get; }

        /// <summary>Número de la pregunta actual (1-15); 0 antes de empezar.</summary>
        int QuestionNumber { get; }

        int TotalQuestions { get; }

        /// <summary>Pregunta actual, o null antes de empezar.</summary>
        QuestionData CurrentQuestion { get; }

        /// <summary>Qué opción original muestra cada cabina.</summary>
        OptionOrder Order { get; }

        /// <summary>Cabina correcta (0-3) una vez revelada; -1 mientras no se sabe.</summary>
        int CorrectBooth { get; }

        RoundOutcome LastRound { get; }

        /// <summary>Multiplicador de la última ronda (1 + apostados en la correcta).</summary>
        int LastMultiplier { get; }

        /// <summary>Nombres de los expulsados en la última ronda, o vacío.</summary>
        string LastEliminated { get; }

        /// <summary>Segundos que quedan de la fase actual (cuenta atrás o revelación); 0 si no tiene tiempo.</summary>
        float PhaseSecondsLeft { get; }

        /// <summary>Dinero total del equipo.</summary>
        long TeamMoney { get; }

        /// <summary>Dinero que se perdería al verificar ahora por estar fuera de las bandejas (regla provisional).</summary>
        long MoneyAtRisk { get; }

        void RequestVerify();
    }
}
