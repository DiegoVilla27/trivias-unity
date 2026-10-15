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

#if UNITY_EDITOR
        public void EditorSet(string language, List<QuestionData> questions)
        {
            _language = language;
            _questions = questions;
        }
#endif
    }
}
