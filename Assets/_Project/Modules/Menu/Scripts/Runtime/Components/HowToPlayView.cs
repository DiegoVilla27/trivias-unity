using TMPro;
using Trivias.Menu.Data;
using Trivias.Menu.Logic;
using Trivias.Shared.Presentation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Tutorial «Cómo se juega»: una tira de seis pasos arriba (se puede saltar a cualquiera), el número de página con sus
    /// puntos, el título de la página, su ilustración, el texto y Anterior / Siguiente (en la última, «Entendido») y Cerrar.
    /// Se abre desde el menú, la sala y la pausa.
    /// </summary>
    public sealed class HowToPlayView : MonoBehaviour
    {
        [SerializeField] private HowToPlayContent _content;
        [SerializeField] private UiBackdrop _backdrop;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _body;
        [Tooltip("Dibuja como teclas las «[E]» del texto.")]
        [SerializeField] private UiKeyedText _bodyKeys;
        [SerializeField] private TextMeshProUGUI _pageLabel;
        [SerializeField] private Image _illustration;
        [Tooltip("Una ilustración por página, en orden.")]
        [SerializeField] private Sprite[] _illustrations;
        [SerializeField] private Button[] _steps;
        [SerializeField] private Image[] _stepCircles;
        [SerializeField] private TextMeshProUGUI[] _stepNumbers;
        [SerializeField] private TextMeshProUGUI[] _stepLabels;
        [SerializeField] private GameObject[] _stepUnderlines;
        [SerializeField] private Image[] _dots;
        [SerializeField] private Sprite _activeCircle;
        [SerializeField] private Sprite _idleCircle;
        [SerializeField] private Color _activeColor = new(0.95f, 0.77f, 0.36f);
        [SerializeField] private Color _idleColor = new(0.62f, 0.66f, 0.78f);
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _nextLabel;
        [SerializeField] private Button _closeButton;

        private UnityAction[] _stepActions;

        public bool IsOpen => _panel.activeSelf;
        public int Page { get; private set; }
        public int PageCount => _content.Count;


        private void Awake()
        {
            _stepActions = new UnityAction[_steps.Length];
            for (var i = 0; i < _steps.Length; i++)
            {
                var page = i;
                _stepActions[i] = () => Show(page);
            }
        }

        private void OnEnable()
        {
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(NextOrClose);
            _closeButton.onClick.AddListener(Close);
            for (var i = 0; i < _steps.Length; i++)
                _steps[i].onClick.AddListener(_stepActions[i]);
        }

        private void OnDisable()
        {
            _previousButton.onClick.RemoveListener(Previous);
            _nextButton.onClick.RemoveListener(NextOrClose);
            _closeButton.onClick.RemoveListener(Close);
            for (var i = 0; i < _steps.Length; i++)
                _steps[i].onClick.RemoveListener(_stepActions[i]);
            _backdrop.Cover(this, false);
        }

        public void Open()
        {
            _panel.SetActive(true);
            _backdrop.Cover(this, true);
            Show(0);
        }

        public void Close()
        {
            _panel.SetActive(false);
            _backdrop.Cover(this, false);
        }

        private void Previous() => Show(Page - 1);

        /// <summary>En la última página el botón «Siguiente» pasa a «Entendido» y cierra el tutorial.</summary>
        private void NextOrClose()
        {
            if (Pager.HasNext(Page, PageCount))
                Show(Page + 1);
            else
                Close();
        }

        public void Show(int page)
        {
            Page = Pager.Clamp(page, PageCount);
            var current = _content[Page];
            _title.text = current.Title.ToUpperInvariant();
            _bodyKeys.SetText(current.Body);
            _pageLabel.text = Pager.Label(Page, PageCount);
            _illustration.sprite = Page < _illustrations.Length ? _illustrations[Page] : null;
            _illustration.type = Image.Type.Simple; // Con su proporción: nunca estirada.
            _illustration.preserveAspect = true;
            _illustration.enabled = _illustration.sprite != null;
            _previousButton.interactable = Pager.HasPrevious(Page);
            _nextLabel.text = Pager.HasNext(Page, PageCount) ? "Siguiente  ›" : "Entendido";

            for (var i = 0; i < _steps.Length; i++)
            {
                var active = i == Page;
                _stepCircles[i].sprite = active ? _activeCircle : _idleCircle;
                _stepNumbers[i].color = active ? new Color(0.06f, 0.09f, 0.19f) : Color.white;
                _stepLabels[i].color = active ? _activeColor : _idleColor;
                _stepLabels[i].text = i < PageCount ? _content[i].Step : string.Empty;
                _stepUnderlines[i].SetActive(active);
            }
            for (var i = 0; i < _dots.Length; i++)
                _dots[i].color = i == Page ? _activeColor : new Color(0.45f, 0.52f, 0.7f);
        }
    }
}
