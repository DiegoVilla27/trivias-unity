using System.Collections.Generic;
using TMPro;
using Trivias.Menu.Logic;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Récords de este ordenador (GDD 12) desde el menú principal: una pestaña por tamaño de grupo (2 a 6 jugadores)
    /// con las mejores victorias. Se redibuja al abrir, al cambiar de pestaña o al entrar un récord nuevo.
    /// </summary>
    public sealed class RecordsView : MonoBehaviour
    {
        private static readonly Color SelectedTab = new(0.95f, 0.70f, 0.22f);
        private static readonly Color IdleTab = new(1f, 1f, 1f, 0.15f);

        [SerializeField] private LocalRecords _records;
        [SerializeField] private GameObject _panel;
        [Tooltip("Una pestaña por tamaño de grupo, de 2 a 6 jugadores, en orden.")]
        [SerializeField] private Button[] _tabs;
        [SerializeField] private TextMeshProUGUI _list;
        [SerializeField] private Button _closeButton;

        private readonly List<RecordEntry> _category = new();
        private UnityAction[] _tabActions;

        public bool IsOpen => _panel.activeSelf;
        public int GroupSize { get; private set; } = RankingRules.MIN_GROUP;
        public string ListText => _list.text;

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
        }

        /// <summary>Abre en la última pestaña vista; si está vacía, en la primera que tenga victorias.</summary>
        public void Open()
        {
            _panel.SetActive(true);
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

        public void Close() => _panel.SetActive(false);

        public void Show(int groupSize)
        {
            GroupSize = Mathf.Clamp(groupSize, RankingRules.MIN_GROUP, RankingRules.MAX_GROUP);
            _records.Category(GroupSize, _category);
            _list.text = RecordsTable.Build(_category, GroupSize);
            for (var i = 0; i < _tabs.Length; i++)
                _tabs[i].image.color = RankingRules.MIN_GROUP + i == GroupSize ? SelectedTab : IdleTab;
        }

        private void OnRecordsChanged()
        {
            if (IsOpen)
                Show(GroupSize);
        }
    }
}
