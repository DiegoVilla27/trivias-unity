using System;
using System.Collections.Generic;
using System.IO;
using Trivias.Questions.Logic;
using UnityEngine;

namespace Trivias.Questions.Data
{
    /// <summary>
    /// Preguntas que ha visto el jugador en este ordenador, de la más antigua a la última. Se guarda en JSON al ver cada
    /// pregunta (un evento por pregunta, nunca por frame). Un archivo dañado no rompe el juego: se empieza de cero.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Questions/Question History", fileName = "QuestionHistory")]
    public sealed class QuestionHistory : ScriptableObject
    {
        private const string FILE_NAME = "trivias-questions-seen.json";

        [Serializable]
        private sealed class SeenData
        {
            public List<string> seen = new();
        }

        public event Action Changed;

        private SeenData _data;
        private string _path;

        public IReadOnlyList<string> Seen => Data.seen;

        private SeenData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        /// <summary>Cambia dónde se guarda (las pruebas usan una carpeta temporal, nunca el historial real).</summary>
        public void UseStoragePath(string path)
        {
            _path = path;
            _data = null;
            EnsureLoaded();
            Changed?.Invoke();
        }

        public void MarkSeen(string questionId)
        {
            if (!QuestionHistoryRules.MarkSeen(Data.seen, questionId))
                return;
            Save();
            Changed?.Invoke();
        }

        /// <summary>Índices en el banco de las preguntas vistas, de la más antigua a la última (se omiten las retiradas).</summary>
        public void CopyIndices(QuestionBank bank, List<ushort> into)
        {
            into.Clear();
            foreach (var id in Data.seen)
            {
                var index = bank.IndexOf(id);
                if (index >= 0 && index <= ushort.MaxValue)
                    into.Add((ushort)index);
            }
        }

        private void EnsureLoaded()
        {
            if (_data != null)
                return;
            _path ??= Path.Combine(Application.persistentDataPath, FILE_NAME);
            _data = Load(_path);
        }

        private static SeenData Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<SeenData>(File.ReadAllText(path));
                    if (data != null)
                    {
                        data.seen ??= new List<string>();
                        return data;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Preguntas] No se pudo leer el historial {path}; se empieza de cero. {exception.Message}");
            }
            return new SeenData();
        }

        /// <summary>Escribe en un temporal y lo sustituye, para no dejar el archivo a medias.</summary>
        private void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
                var temporary = _path + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(_data));
                if (File.Exists(_path))
                    File.Delete(_path);
                File.Move(temporary, _path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Preguntas] No se pudo guardar el historial {_path}. {exception.Message}");
            }
        }

        private void OnEnable()
        {
            _data = null;
            _path = null;
        }
    }
}
