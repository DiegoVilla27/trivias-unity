using System;
using System.Collections.Generic;
using Trivias.Presenter.Logic;
using UnityEngine;

namespace Trivias.Presenter.Data
{
    /// <summary>Frase del presentador ya importada y validada desde el contenido editable.</summary>
    [Serializable]
    public sealed class PhraseData
    {
        [SerializeField] private string _id;
        [SerializeField] private PresenterEvent _event;
        [SerializeField, TextArea] private string _text;

        public PhraseData(string id, PresenterEvent presenterEvent, string text)
        {
            _id = id;
            _event = presenterEvent;
            _text = text;
        }

        public string Id => _id;
        public PresenterEvent Event => _event;
        public string Text => _text;
    }

    /// <summary>
    /// Catálogo de frases (GDD 9.1), importado y validado en el editor desde <c>Content/phrases_*.json</c>.
    /// Añadir frases no requiere tocar código.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Presenter/Presenter Catalog", fileName = "PresenterCatalog")]
    public sealed class PresenterCatalog : ScriptableObject
    {
        [SerializeField] private string _language = "es";
        [SerializeField] private List<PhraseData> _phrases = new();

        public int Count => _phrases.Count;
        public PhraseData this[int index] => _phrases[index];

        /// <summary>Índices de frase agrupados por evento (se calcula una vez, al cargar).</summary>
        public IReadOnlyList<int>[] BuildIndexByEvent()
        {
            var byEvent = new List<int>[PresenterEvents.COUNT];
            for (var e = 0; e < byEvent.Length; e++)
                byEvent[e] = new List<int>();
            for (var i = 0; i < _phrases.Count; i++)
                byEvent[(int)_phrases[i].Event].Add(i);
            return byEvent;
        }

#if UNITY_EDITOR
        public void EditorSet(string language, List<PhraseData> phrases)
        {
            _language = language;
            _phrases = phrases;
        }
#endif
    }
}
