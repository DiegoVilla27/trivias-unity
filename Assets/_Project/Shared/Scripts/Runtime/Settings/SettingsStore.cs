using System;
using System.IO;
using UnityEngine;

namespace Trivias.Shared.Settings
{
    /// <summary>Lee y guarda las opciones. Un archivo dañado no rompe el juego: se vuelve a las de fábrica.</summary>
    public static class SettingsStore
    {
        public static SettingsData Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<SettingsData>(File.ReadAllText(path));
                    if (data != null)
                    {
                        data.Normalize();
                        return data;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Opciones] No se pudieron leer {path}; se usan las de fábrica. {exception.Message}");
            }

            var fresh = new SettingsData();
            fresh.Normalize();
            return fresh;
        }

        public static void Save(string path, SettingsData data)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temporary, path);
        }
    }
}
