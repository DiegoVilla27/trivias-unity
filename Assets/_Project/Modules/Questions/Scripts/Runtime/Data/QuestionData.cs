using System;
using UnityEngine;

namespace Trivias.Questions.Data
{
    /// <summary>Una pregunta de cultura general (CONTENIDO.md). Las opciones están en su orden original.</summary>
    [Serializable]
    public sealed class QuestionData
    {
        public const int OPTION_COUNT = 4;

        [SerializeField] private string _id;
        [SerializeField] private string _category;
        [SerializeField, Range(1, 3)] private int _difficulty = 1;
        [SerializeField, TextArea] private string _text;
        [SerializeField] private string[] _options = new string[OPTION_COUNT];
        [SerializeField, Range(0, OPTION_COUNT - 1)] private int _correctOption;
        [SerializeField, TextArea] private string _explanation;
        [SerializeField] private string _source;

        public QuestionData(string id, string category, int difficulty, string text, string[] options,
            int correctOption, string explanation, string source)
        {
            _id = id;
            _category = category;
            _difficulty = difficulty;
            _text = text;
            _options = options;
            _correctOption = correctOption;
            _explanation = explanation;
            _source = source;
        }

        public string Id => _id;
        public string Category => _category;
        public int Difficulty => _difficulty;
        public string Text => _text;
        public string Option(int originalIndex) => _options[originalIndex];
        public int CorrectOption => _correctOption;
        public string Explanation => _explanation;
        public string Source => _source;
    }
}
