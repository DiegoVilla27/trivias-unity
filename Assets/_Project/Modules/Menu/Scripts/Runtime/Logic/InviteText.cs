using System.Collections.Generic;
using System.Text;
using Trivias.Shared.Rooms;

namespace Trivias.Menu.Logic
{
    /// <summary>Texto de «Invita a tus amigos» en la sala de espera del host (C# puro, probado en EditMode).</summary>
    public static class InviteText
    {
        public const string GUIDE = "docs/JUGAR_CON_AMIGOS.md";

        /// <summary>La dirección que conviene compartir (la primera de la lista), o vacío.</summary>
        public static string Primary(IReadOnlyList<RoomAddress> addresses) =>
            addresses == null || addresses.Count == 0 ? string.Empty : addresses[0].Address;

        public static string Build(IReadOnlyList<RoomAddress> addresses)
        {
            if (addresses == null || addresses.Count == 0)
                return string.Empty;

            var text = new StringBuilder();
            var first = addresses[0];
            text.Append("Pásales esta dirección:\n<size=140%><b>").Append(first.Address).Append("</b></size>\n")
                .Append("<size=80%>").Append(Describe(first)).Append("</size>");

            if (addresses.Count > 1)
            {
                text.Append("\n<size=70%>Otras: ");
                for (var i = 1; i < addresses.Count; i++)
                    text.Append(i > 1 ? " · " : string.Empty).Append(addresses[i].Address).Append(" (").Append(Describe(addresses[i])).Append(')');
                text.Append("</size>");
            }

            text.Append("\n<size=70%><i>").Append(Hint(first.Kind)).Append("</i></size>");
            return text.ToString();
        }

        private static string Describe(in RoomAddress address) => address.Kind switch
        {
            RoomAddressKind.VirtualNetwork => $"red virtual {address.NetworkName}".TrimEnd(),
            RoomAddressKind.LocalNetwork => "misma wifi o red local",
            _ => "solo este ordenador",
        };

        private static string Hint(RoomAddressKind kind) => kind switch
        {
            RoomAddressKind.VirtualNetwork => "Tus amigos deben estar unidos a la misma red virtual. Ellos la pegan en «Unirme a una sala».",
            RoomAddressKind.LocalNetwork => $"Sirve si estáis en la misma wifi. Para jugar por internet usad una red virtual gratuita (guía: {GUIDE}).",
            _ => $"No estás conectado a ninguna red: solo se puede jugar en este ordenador (guía: {GUIDE}).",
        };
    }
}
