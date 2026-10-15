using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using Trivias.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.Questions.Editor
{
    /// <summary>
    /// Importa el contenido editable (JSON) a un asset QuestionBank y lo valida. Tiempo de edición:
    /// si hay errores, el build se detiene con la lista de problemas.
    /// </summary>
    public static class QuestionBankBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Questions";
        private const string SOURCE_PATH = MODULE_ROOT + "/Content/questions_es.json";
        public const string BANK_PATH = MODULE_ROOT + "/Config/QuestionBank_es.asset";

        [Serializable] private sealed class OptionDto { public string id; public string text; }

        [Serializable]
        private sealed class QuestionDto
        {
            public string id;
            public string category;
            public int difficulty;
            public string text;
            public OptionDto[] options;
            public string correctOptionId;
            public string explanation;
            public string source;
        }

        [Serializable] private sealed class FileDto { public string language; public QuestionDto[] questions; }

        [MenuItem("Tools/Trivias/Build/Question Bank")]
        public static QuestionBank Build()
        {
            var json = File.ReadAllText(SOURCE_PATH);
            var file = JsonUtility.FromJson<FileDto>(json);
            if (file?.questions == null || file.questions.Length == 0)
                throw new InvalidOperationException($"{SOURCE_PATH} no contiene preguntas.");

            var entries = file.questions.Select(q => new QuestionValidator.Entry(
                q.id, q.text, q.difficulty,
                q.options?.Select(o => o.id).ToArray(),
                q.options?.Select(o => o.text).ToArray(),
                q.correctOptionId, q.explanation)).ToList();

            var errors = QuestionValidator.Validate(entries);
            if (errors.Count > 0)
                throw new InvalidOperationException($"Preguntas con errores en {SOURCE_PATH}:\n- {string.Join("\n- ", errors)}");

            var questions = new List<QuestionData>(file.questions.Length);
            foreach (var q in file.questions)
            {
                var correct = Array.FindIndex(q.options, o => o.id == q.correctOptionId);
                questions.Add(new QuestionData(q.id, q.category, q.difficulty, q.text,
                    q.options.Select(o => o.text).ToArray(), correct, q.explanation, q.source));
            }

            var bank = EditorAssets.GetOrCreateAsset<QuestionBank>(BANK_PATH);
            bank.EditorSet(file.language, questions);
            EditorUtility.SetDirty(bank);
            Debug.Log($"[Preguntas] {questions.Count} preguntas importadas y validadas " +
                      $"(fáciles {Count(questions, 1)}, medias {Count(questions, 2)}, difíciles {Count(questions, 3)}).");
            return bank;
        }

        private static int Count(List<QuestionData> questions, int difficulty) =>
            questions.Count(q => q.Difficulty == difficulty);
    }
}
