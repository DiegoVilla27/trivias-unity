using System;
using System.IO;
using UnityEngine;

namespace Trivias.Shared.Profile
{
    /// <summary>Lee y guarda el perfil como JSON. Un archivo dañado no rompe el juego: se empieza con un perfil nuevo.</summary>
    public static class ProfileStore
    {
        public static ProfileData Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<ProfileData>(File.ReadAllText(path));
                    if (data != null)
                    {
                        data.Normalize();
                        return data;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Perfil] No se pudo leer {path}; se empieza un perfil nuevo. {exception.Message}");
            }

            var fresh = new ProfileData();
            fresh.Normalize();
            return fresh;
        }

        /// <summary>Guarda de forma segura: escribe en un temporal y lo sustituye, para no dejar el archivo a medias.</summary>
        public static void Save(string path, ProfileData data)
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
