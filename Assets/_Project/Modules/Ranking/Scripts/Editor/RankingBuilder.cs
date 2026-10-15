using Trivias.Ranking.Data;
using Trivias.Shared.Editor;

namespace Trivias.Ranking.Editor
{
    /// <summary>Crea el anclaje de los récords locales (lo usan los resultados y el menú).</summary>
    public static class RankingBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Ranking";
        public const string LOCAL_RECORDS_PATH = MODULE_ROOT + "/Config/LocalRecords.asset";

        public static LocalRecords LocalRecords() => EditorAssets.GetOrCreateAsset<LocalRecords>(LOCAL_RECORDS_PATH);
    }
}
