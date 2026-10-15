using System;
using System.Collections.Generic;

namespace Trivias.Shared.Profile
{
    /// <summary>Progreso guardado del jugador en este ordenador (GDD 11): nombre, fichas, compras y lo que lleva puesto.</summary>
    [Serializable]
    public sealed class ProfileData
    {
        /// <summary>Nombre elegido en el menú (vacío: «Jugador N»). Con Steam se usará el de la cuenta.</summary>
        public string name = string.Empty;
        public int tokens;
        public List<string> owned = new();
        public string[] equipped = new string[CosmeticSlots.COUNT];

        /// <summary>Partidas cuyas fichas ya se entregaron (para no darlas dos veces).</summary>
        public List<string> awardedMatches = new();

        public CosmeticLoadout Loadout => new(Equipped(CosmeticSlot.Head), Equipped(CosmeticSlot.Face),
            Equipped(CosmeticSlot.Neck), Equipped(CosmeticSlot.Color));

        public string Equipped(CosmeticSlot slot) =>
            equipped != null && (int)slot < equipped.Length ? equipped[(int)slot] ?? string.Empty : string.Empty;

        /// <summary>Corrige datos incompletos de un archivo antiguo o editado a mano.</summary>
        public void Normalize()
        {
            name = PlayerNameRules.Sanitize(name);
            owned ??= new List<string>();
            awardedMatches ??= new List<string>();
            if (equipped == null || equipped.Length != CosmeticSlots.COUNT)
            {
                var fixedSlots = new string[CosmeticSlots.COUNT];
                for (var i = 0; equipped != null && i < equipped.Length && i < fixedSlots.Length; i++)
                    fixedSlots[i] = equipped[i];
                equipped = fixedSlots;
            }
            if (tokens < 0)
                tokens = 0;
        }
    }
}
