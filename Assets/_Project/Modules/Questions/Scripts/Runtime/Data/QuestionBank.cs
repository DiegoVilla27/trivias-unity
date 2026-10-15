using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Questions.Data
{
    /// <summary>
    /// Banco de preguntas ya importado y validado en el editor desde el JSON de contenido.
    /// En runtime solo se lee: sin parseo ni validación durante la partida.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Questions/Question Bank", fileName = "QuestionBank")]
    public sealed class QuestionBank : ScriptableObject
    {
        [SerializeField] private string _language = "es";
        [SerializeField] private List<QuestionData> _questions = new();

        public string Language => _language;
        public IReadOnlyList<QuestionData> Questions => _questions;
        public int Count => _questions.Count;
        public QuestionData this[int index] => _questions[index];

        [NonSerialized] private Dictionary<string, int> _indexById;

        /// <summary>Posición de una pregunta por su id, o -1 si ya no está en el banco (se calcula una vez).</summary>
        public int IndexOf(string id)
        {
            if (_indexById == null)
            {
                _indexById = new Dictionary<string, int>(_questions.Count);
                for (var i = 0; i < _questions.Count; i++)
                    _indexById[_questions[i].Id] = i;
            }
            return id != null && _indexById.TryGetValue(id, out var index) ? index : -1;
        }

        /// <summary>Categoría de cada pregunta como número (misma categoría, mismo número), para variar el orden.</summary>
        public int[] CategoryNumbers()
        {
            var numbers = new int[_questions.Count];
            var byName = new Dictionary<string, int>();
            for (var i = 0; i < _questions.Count; i++)
            {
                var category = _questions[i].Category ?? string.Empty;
                if (!byName.TryGetValue(category, out var number))
                {
                    number = byName.Count;
                    byName[category] = number;
                }
                numbers[i] = number;
            }
            return numbers;
        }

#if UNITY_EDITOR
        public void EditorSet(string language, List<QuestionData> questions)
        {
            _language = language;
            _questions = questions;
            _indexById = null;
        }
#endif
    }
}
