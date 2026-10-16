using System;
using Trivias.Match.Logic;

namespace Trivias.Feedback.Logic
{
    /// <summary>Cómo reacciona el público.</summary>
    public enum CrowdReaction : byte
    {
        None = 0,
        Applause = 1,
        Cheer = 2,
        Aww = 3,
    }

    /// <summary>
    /// El plató vivo (GDD 13): reacción del público, brillo de las bombillas de cabinas y marquesina, y balanceo de los
    /// focos, según el momento del concurso. C# puro, probado en EditMode.
    /// </summary>
    public static class StageShowRules
    {
        /// <summary>Bombillas que no son de ninguna cabina (la marquesina de la pantalla).</summary>
        public const int MARQUEE = -1;

        /// <summary>Reacción del público al cambiar de fase (una sola vez por cambio).</summary>
        public static CrowdReaction Reaction(in MatchSnapshot previous, in MatchSnapshot current)
        {
            if (current.Phase == previous.Phase)
                return CrowdReaction.None;

            switch (current.Phase)
            {
                case MatchPhase.Betting:
                    return previous.Phase == MatchPhase.Lobby ? CrowdReaction.Applause : CrowdReaction.None;
                case MatchPhase.Reveal:
                    return current.SomeoneExpelled || current.Kept <= 0 ? CrowdReaction.Aww : CrowdReaction.Applause;
                case MatchPhase.Victory:
                    return CrowdReaction.Cheer;
                case MatchPhase.Defeat:
                    return CrowdReaction.Aww;
                default:
                    return CrowdReaction.None;
            }
        }

        /// <summary>
        /// Brillo de una bombilla (1 = el del material). <paramref name="booth"/> es la cabina (0..3) o
        /// <see cref="MARQUEE"/>; <paramref name="order"/>, su posición en la fila (para las persecuciones).
        /// </summary>
        public static float BulbLevel(StageMood mood, int booth, int correctBooth, int order, float time)
        {
            switch (mood)
            {
                case StageMood.Countdown:
                    return (int)(time * 6f) % 2 == 0 ? 1f : 0.1f; // Parpadeo con la cuenta atrás.
                case StageMood.Correct:
                case StageMood.Wrong:
                    if (booth == MARQUEE)
                        return mood == StageMood.Correct ? 0.3f + 0.7f * Chase(order, time, 12f) : 0.2f;
                    if (booth != correctBooth)
                        return 0.06f; // Las cabinas falsas se apagan…
                    return 1.05f + 0.25f * (float)Math.Sin(time * 10f); // …y la correcta brilla y late.
                case StageMood.Victory:
                    return 0.3f + 0.8f * Chase(order + booth * 3, time, 16f);
                case StageMood.Defeat:
                    return 0.15f + 0.08f * (float)Math.Sin(time * 2.3f + order);
                case StageMood.Hard:
                    return 0.3f + 0.3f * Chase(order, time, 5f);
                default:
                    return 0.45f + 0.35f * Chase(order, time, 4f); // Persecución suave mientras se juega.
            }
        }

        /// <summary>Giro (grados) de un foco que se balancea: suave siempre, amplio y rápido en la victoria.</summary>
        public static float SwayDegrees(int index, float time, bool party) =>
            (party ? 26f : 9f) * (float)Math.Sin(time * (party ? 1.7f : 0.35f) + index * 1.3f);

        /// <summary>Onda que recorre una fila de bombillas (0..1).</summary>
        public static float Chase(int order, float time, float speed) =>
            0.5f + 0.5f * (float)Math.Sin(order * 0.9f - time * speed);
    }
}
