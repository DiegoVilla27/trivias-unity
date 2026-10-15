using System;
using System.IO;
using Trivias.Ranking.Logic;
using UnityEngine;

namespace Trivias.Ranking.Data
{
    /// <summary>Lee y guarda los récords como JSON. Un archivo dañado no rompe el juego: se empieza sin récords.</summary>
    public static class RecordStore
    {
        public static RecordBook Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var book = JsonUtility.FromJson<RecordBook>(File.ReadAllText(path));
                    if (book != null)
                    {
                        book.Normalize();
                        return book;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Récords] No se pudo leer {path}; se empieza sin récords. {exception.Message}");
            }

            var fresh = new RecordBook();
            fresh.Normalize();
            return fresh;
        }

        /// <summary>Guarda de forma segura: escribe en un temporal y lo sustituye, para no dejar el archivo a medias.</summary>
        public static void Save(string path, RecordBook book)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(book, true));
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temporary, path);
        }
    }
}
