using System.Collections.Generic;
using System.Text;
using TMPro;
using Trivias.Menu.Logic;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using Trivias.Shared.Presentation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Récords de este ordenador (GDD 12): una pestaña por tamaño de grupo (2 a 6 jugadores) y una tabla con puesto
    /// (medallas en los tres primeros), premio, tiempo, fecha, participantes y supervivientes. Se redibuja al abrir, al
    /// cambiar de pestaña o al entrar un récord nuevo.
    /// </summary>
    public sealed class RecordsView : MonoBehaviour
    {
        [SerializeField] private LocalRecords _records;
        [SerializeField] private UiBackdrop _backdrop;
        [SerializeField] private GameObject _panel;
        [Tooltip("Una pestaña por tamaño de grupo, de 2 a 6 jugadores, en orden.")]
        [SerializeField] private Button[] _tabs;
        [SerializeField] private TextMeshProUGUI[] _tabLabels;
        [SerializeField] private Sprite _selectedTab;
        [SerializeField] private Sprite _idleTab;
        [SerializeField] private Color _selectedTabText = new(0.06f, 0.09f, 0.19f);
        [SerializeField] private Color _idleTabText = Color.white;
        [SerializeField] private RecordRowView[] _rows;
        [Tooltip("Oro, plata y bronce.")]
        [SerializeField] private Sprite[] _medals = new Sprite[3];
        [SerializeField] private TextMeshProUGUI _empty;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Button _closeButton;

        private readonly List<RecordEntry> _category = new();
        private readonly List<RecordRow> _table = new();
        private readonly StringBuilder _text = new();
        private UnityAction[] _tabActions;

        public bool IsOpen => _panel.activeSelf;
        public int GroupSize { get; private set; } = RankingRules.MIN_GROUP;

        /// <summary>La tabla en texto, una fila por línea (para las pruebas).</summary>
        public string ListText
        {
            get
            {
                if (_table.Count == 0)
                    return _empty.text;
                _text.Clear();
                for (var i = 0; i < _table.Count; i++)
                {
                    var row = _table[i];
                    _text.Append(i > 0 ? "\n" : string.Empty).Append(row.Position).Append(". ").Append(row.Money).Append(' ')
                        .Append(row.Time).Append(' ').Append(row.Date).Append(' ').Append(row.Players).Append(' ').Append(row.Survivors);
                }
                return _text.ToString();
            }
        }


        private void Awake()
        {
            _tabActions = new UnityAction[_tabs.Length];
            for (var i = 0; i < _tabs.Length; i++)
            {
                var groupSize = RankingRules.MIN_GROUP + i;
                _tabActions[i] = () => Show(groupSize);
            }
        }

        private void OnEnable()
        {
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].onClick.AddListener(_tabActions[i]);
            _closeButton.onClick.AddListener(Close);
            _records.Changed += OnRecordsChanged;
        }

        private void OnDisable()
        {
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].onClick.RemoveListener(_tabActions[i]);
            _closeButton.onClick.RemoveListener(Close);
            _records.Changed -= OnRecordsChanged;
            _backdrop.Cover(this, false);
        }

        /// <summary>Abre en la última pestaña vista; si está vacía, en la primera que tenga victorias.</summary>
        public void Open()
        {
            _panel.SetActive(true);
            _backdrop.Cover(this, true);
            _records.Category(GroupSize, _category);
            if (_category.Count == 0)
            {
                for (var size = RankingRules.MIN_GROUP; size <= RankingRules.MAX_GROUP; size++)
                {
                    _records.Category(size, _category);
                    if (_category.Count == 0)
                        continue;
                    GroupSize = size;
                    break;
                }
            }
            Show(GroupSize);
        }

        public void Close()
        {
            _panel.SetActive(false);
            _backdrop.Cover(this, false);
        }

        public void Show(int groupSize)
        {
            GroupSize = Mathf.Clamp(groupSize, RankingRules.MIN_GROUP, RankingRules.MAX_GROUP);
            _records.Category(GroupSize, _category);
            RecordsTable.Rows(_category, GroupSize, _table);
            for (var i = 0; i < _rows.Length; i++)
            {
                if (i >= _table.Count)
                {
                    _rows[i].Hide();
                    continue;
                }
                var row = _table[i];
                var medal = row.Position >= 1 && row.Position <= _medals.Length ? _medals[row.Position - 1] : null;
                _rows[i].Show(row.Position, medal, row.Money, row.Time, row.Date, row.Players, row.Survivors);
            }
            _empty.gameObject.SetActive(_table.Count == 0);
            _empty.text = RecordsTable.EMPTY;
            _scroll.verticalNormalizedPosition = 1f;

            for (var i = 0; i < _tabs.Length; i++)
            {
                var selected = RankingRules.MIN_GROUP + i == GroupSize;
                _tabs[i].image.sprite = selected ? _selectedTab : _idleTab;
                _tabLabels[i].color = selected ? _selectedTabText : _idleTabText;
            }
        }

        private void OnRecordsChanged()
        {
            if (IsOpen)
                Show(GroupSize);
        }
    }
}
