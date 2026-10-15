using System.IO;
using NUnit.Framework;
using Trivias.Questions.Data;
using Trivias.Ranking.Data;
using Trivias.Shared.Profile;
using Trivias.Shared.Settings;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Antes de cualquier prueba en juego, el perfil (nombre, fichas, compras), las opciones, los récords y el historial
    /// de preguntas se guardan en una carpeta temporal: las pruebas nunca tocan los datos reales del jugador.
    /// </summary>
    [SetUpFixture]
    public sealed class TemporaryProfileSetup
    {
        private const string PROFILE_ASSET = "Assets/_Project/Shared/Config/Profile/PlayerProfile.asset";
        private const string SETTINGS_ASSET = "Assets/_Project/Shared/Config/Settings/GameSettings.asset";
        private const string RECORDS_ASSET = "Assets/_Project/Modules/Ranking/Config/LocalRecords.asset";
        private const string HISTORY_ASSET = "Assets/_Project/Modules/Questions/Config/QuestionHistory.asset";
        public static string Folder { get; private set; }

        [OneTimeSetUp]
        public void UseTemporaryProfile()
        {
            Folder = Path.Combine(Path.GetTempPath(), "trivias-playmode-tests", Path.GetRandomFileName());
            Profile().UseStoragePath(Path.Combine(Folder, "profile.json"));
            Settings().UseStoragePath(Path.Combine(Folder, "settings.json"));
            Records().UseStoragePath(Path.Combine(Folder, "records.json"));
            History().UseStoragePath(Path.Combine(Folder, "questions-seen.json"));
        }

        [OneTimeTearDown]
        public void DeleteTemporaryProfile()
        {
            if (Directory.Exists(Folder))
                Directory.Delete(Folder, true);
        }

        public static PlayerProfile Profile()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerProfile>(PROFILE_ASSET);
#else
            return null;
#endif
        }

        public static GameSettings Settings()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameSettings>(SETTINGS_ASSET);
#else
            return null;
#endif
        }

        public static LocalRecords Records()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<LocalRecords>(RECORDS_ASSET);
#else
            return null;
#endif
        }

        public static QuestionHistory History()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<QuestionHistory>(HISTORY_ASSET);
#else
            return null;
#endif
        }

        /// <summary>Historial de preguntas vacío en un archivo propio de la prueba.</summary>
        public static QuestionHistory FreshHistory()
        {
            var history = History();
            history.UseStoragePath(Path.Combine(Folder, Path.GetRandomFileName() + "-seen.json"));
            return history;
        }

        /// <summary>Récords vacíos en un archivo propio de la prueba.</summary>
        public static LocalRecords FreshRecords()
        {
            var records = Records();
            records.UseStoragePath(Path.Combine(Folder, Path.GetRandomFileName() + "-records.json"));
            return records;
        }

        /// <summary>Opciones de fábrica en un archivo propio de la prueba.</summary>
        public static GameSettings FreshSettings()
        {
            var settings = Settings();
            settings.UseStoragePath(Path.Combine(Folder, Path.GetRandomFileName() + "-settings.json"));
            return settings;
        }

        /// <summary>Empieza cada prueba con un perfil vacío y propio.</summary>
        public static PlayerProfile FreshProfile()
        {
            var profile = Profile();
            profile.UseStoragePath(Path.Combine(Folder, Path.GetRandomFileName() + ".json"));
            return profile;
        }
    }
}
