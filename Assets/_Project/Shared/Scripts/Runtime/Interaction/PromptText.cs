namespace Trivias.Shared.Interaction
{
    /// <summary>Formato común de los avisos de interacción: tecla resaltada y acción corta («E Coger fajo»).</summary>
    public static class PromptText
    {
        public const string SEPARATOR = "      ";
        private const string KEY_COLOR = "#FFD15C";

        public static string Key(string key, string action) => $"<b><color={KEY_COLOR}>{key}</color></b> {action}";

        public static string Join(string first, string second) =>
            string.IsNullOrEmpty(first) ? second : string.IsNullOrEmpty(second) ? first : first + SEPARATOR + second;
    }
}
