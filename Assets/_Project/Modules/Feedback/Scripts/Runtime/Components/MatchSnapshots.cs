using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>Lee de la partida lo que necesitan el sonido y el confeti (sin allocations).</summary>
    internal static class MatchSnapshots
    {
        public static MatchSnapshot From(IMatchSession session) => session == null
            ? MatchSnapshot.Lobby
            : new MatchSnapshot(session.Phase, session.QuestionNumber, Mathf.CeilToInt(session.PhaseSecondsLeft),
                session.IsBettingTimed, session.LastRound.Kept, session.LastMultiplier,
                !string.IsNullOrEmpty(session.LastEliminated));
    }
}
