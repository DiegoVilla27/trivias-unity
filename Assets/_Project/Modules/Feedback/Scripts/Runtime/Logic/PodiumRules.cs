using System;
using System.Collections.Generic;

namespace Trivias.Feedback.Logic
{
    /// <summary>Una placa del podio de la victoria.</summary>
    public readonly struct PodiumPlaque
    {
        public readonly string Name;
        public readonly bool Survived;

        public PodiumPlaque(string name, bool survived)
        {
            Name = name;
            Survived = survived;
        }
    }

    /// <summary>Podio de la victoria (C# puro, probado en EditMode).</summary>
    public static class PodiumRules
    {
        public const int MAX_PLAQUES = 6;
        private static readonly string[] Separator = { ", " };

        /// <summary>Supervivientes primero (placa dorada) y después expulsados (placa gris); como mucho 6.</summary>
        public static void Plaques(string survivors, string eliminated, List<PodiumPlaque> plaques)
        {
            plaques.Clear();
            Add(survivors, true, plaques);
            Add(eliminated, false, plaques);
        }

        /// <summary>Altura del podio mientras sube desde debajo del suelo (frena al llegar).</summary>
        public static float LiftHeight(float elapsed, float duration, float depth)
        {
            if (duration <= 0f || elapsed >= duration)
                return 0f;
            var t = Math.Max(0f, elapsed / duration);
            var eased = 1f - (1f - t) * (1f - t) * (1f - t);
            return -depth * (1f - eased);
        }

        private static void Add(string names, bool survived, List<PodiumPlaque> plaques)
        {
            if (string.IsNullOrWhiteSpace(names))
                return;
            foreach (var name in names.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (plaques.Count >= MAX_PLAQUES)
                    return;
                var trimmed = name.Trim();
                if (trimmed.Length > 0)
                    plaques.Add(new PodiumPlaque(trimmed, survived));
            }
        }
    }
}
