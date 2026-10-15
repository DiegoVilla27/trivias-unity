using TMPro;
using Trivias.Menu.Data;
using Trivias.Menu.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>Tutorial «Cómo se juega»: páginas con anterior/siguiente. Se abre desde el menú, la sala y la pausa.</summary>
    public sealed class HowToPlayView : MonoBehaviour
    {
        [SerializeField] private HowToPlayContent _content;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _body;
        [SerializeField] private TextMeshProUGUI _pageLabel;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _nextLabel;
        [SerializeField] private Button _closeButton;

        public bool IsOpen => _panel.activeSelf;
        public int Page { get; private set; }
        public int PageCount => _content.Count;

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(NextOrClose);
            _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(Previous);
            _nextButton.onClick.RemoveListener(NextOrClose);
            _closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            _panel.SetActive(true);
            Show(0);
        }

        public void Close() => _panel.SetActive(false);

        private void Previous() => Show(Page - 1);

        /// <summary>En la última página el botón «Siguiente» pasa a «¡A jugar!» y cierra el tutorial.</summary>
        private void NextOrClose()
        {
            if (Pager.HasNext(Page, PageCount))
                Show(Page + 1);
            else
                Close();
        }

        private void Show(int page)
        {
            Page = Pager.Clamp(page, PageCount);
            var current = _content[Page];
            _title.text = current.Title;
            _body.text = current.Body;
            _pageLabel.text = Pager.Label(Page, PageCount);
            _previousButton.interactable = Pager.HasPrevious(Page);
            _nextLabel.text = Pager.HasNext(Page, PageCount) ? "Siguiente" : "¡A jugar!";
        }
    }
}
