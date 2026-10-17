using System.Globalization;

namespace Trivias.Shop.Logic
{
    /// <summary>Cómo está un artículo para este jugador.</summary>
    public enum OfferState
    {
        /// <summary>Personaje gratuito, disponible para todos.</summary>
        Free,
        /// <summary>Comprado (personaje) o adquirido (complemento).</summary>
        Owned,
        /// <summary>Hay que comprarlo.</summary>
        Locked,
        /// <summary>El personaje que lleva.</summary>
        Chosen,
        /// <summary>El complemento que lleva puesto.</summary>
        Equipped,
    }

    /// <summary>Qué se ve de un artículo y qué hace su botón.</summary>
    public readonly struct ShopOffer
    {
        public readonly OfferState State;
        /// <summary>Texto de la pastilla de la tarjeta («Gratis», «1.200 fichas», «Adquirido»…).</summary>
        public readonly string Badge;
        /// <summary>Texto del botón principal («Comprar y elegir», «Elegir», «Te faltan 300 fichas»…).</summary>
        public readonly string Action;
        public readonly bool CanAct;

        public ShopOffer(OfferState state, string badge, string action, bool canAct)
        {
            State = state;
            Badge = badge;
            Action = action;
            CanAct = canAct;
        }
    }

    /// <summary>
    /// Reglas de la tienda (C# puro, con pruebas), como en los bocetos: comprar un personaje lo deja elegido; uno que ya
    /// se tiene se elige; el elegido no se puede volver a elegir. Un complemento se compra y se pone; uno adquirido se
    /// pone o se quita. Si faltan fichas, el precio sigue a la vista y el botón dice cuántas faltan.
    /// </summary>
    public static class ShopRules
    {
        private static readonly NumberFormatInfo Thousands = new() { NumberGroupSeparator = ".", NumberGroupSizes = new[] { 3 } };

        /// <summary>«1.200 fichas».</summary>
        public static string Tokens(int amount) => $"{amount.ToString("N0", Thousands)} fichas";

        public static ShopOffer Character(bool free, bool owned, bool chosen, int price, int tokens)
        {
            if (chosen)
                return new ShopOffer(OfferState.Chosen, "Elegido", "Elegido", false);
            if (free)
                return new ShopOffer(OfferState.Free, "Gratis", "Elegir", true);
            if (owned)
                return new ShopOffer(OfferState.Owned, "Comprado", "Elegir", true);
            return tokens >= price
                ? new ShopOffer(OfferState.Locked, Tokens(price), "Comprar y elegir", true)
                : new ShopOffer(OfferState.Locked, Tokens(price), $"Te faltan {Tokens(price - tokens)}", false);
        }

        public static ShopOffer Accessory(bool owned, bool equipped, int price, int tokens)
        {
            if (equipped)
                return new ShopOffer(OfferState.Equipped, "Puesto", "Quitar", true);
            if (owned)
                return new ShopOffer(OfferState.Owned, "Adquirido", "Equipar", true);
            return tokens >= price
                ? new ShopOffer(OfferState.Locked, Tokens(price), "Comprar y equipar", true)
                : new ShopOffer(OfferState.Locked, Tokens(price), $"Te faltan {Tokens(price - tokens)}", false);
        }

        /// <summary>Paso de las flechas (dando la vuelta).</summary>
        public static int Step(int index, int direction, int count) =>
            count <= 0 ? 0 : ((index + (direction >= 0 ? 1 : -1)) % count + count) % count;
    }
}
