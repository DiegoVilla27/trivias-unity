using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Menu.Data
{
    [Serializable]
    public sealed class HowToPlayPage
    {
        [SerializeField] private string _title;
        [SerializeField, TextArea(4, 12)] private string _body;

        public HowToPlayPage(string title, string body)
        {
            _title = title;
            _body = body;
        }

        public string Title => _title;
        public string Body => _body;
    }

    /// <summary>
    /// Páginas del tutorial «Cómo se juega». Se importan de <c>Menu/Content/howtoplay_es.json</c> al construir
    /// (editable sin tocar código) y se validan en las pruebas de contenido.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Menu/How To Play", fileName = "HowToPlay")]
    public sealed class HowToPlayContent : ScriptableObject
    {
        [SerializeField] private List<HowToPlayPage> _pages = new();

        public int Count => _pages.Count;
        public HowToPlayPage this[int index] => _pages[index];

#if UNITY_EDITOR
        public void EditorSet(List<HowToPlayPage> pages) => _pages = pages;
#endif
    }
}
