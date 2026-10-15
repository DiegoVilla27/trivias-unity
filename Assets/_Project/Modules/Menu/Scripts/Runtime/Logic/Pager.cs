namespace Trivias.Menu.Logic
{
    /// <summary>Paginación del tutorial (C# puro, probado en EditMode).</summary>
    public static class Pager
    {
        public static int Clamp(int page, int count) => count <= 0 ? 0 : page < 0 ? 0 : page >= count ? count - 1 : page;
        public static bool HasPrevious(int page) => page > 0;
        public static bool HasNext(int page, int count) => page < count - 1;
        public static string Label(int page, int count) => count <= 0 ? string.Empty : $"{page + 1} / {count}";
    }
}
