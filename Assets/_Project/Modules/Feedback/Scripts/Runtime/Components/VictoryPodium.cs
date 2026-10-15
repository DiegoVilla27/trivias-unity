using System.Collections.Generic;
using TMPro;
using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Podio dorado de la victoria: sube delante de la pantalla con una placa por concursante (supervivientes en dorado,
    /// expulsados en gris). Es solo decorado: no tiene colisiones, así que no empuja a nadie al subir.
    /// </summary>
    public sealed class VictoryPodium : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private GameObject _podium;
        [SerializeField] private Transform _lift;
        [SerializeField] private GameObject[] _plaques = new GameObject[PodiumRules.MAX_PLAQUES];
        [SerializeField] private TextMeshPro[] _plaqueNames = new TextMeshPro[PodiumRules.MAX_PLAQUES];
        [SerializeField] private Renderer[] _plaqueRenderers = new Renderer[PodiumRules.MAX_PLAQUES];
        [SerializeField] private Material _survivorMaterial;
        [SerializeField] private Material _eliminatedMaterial;
        [SerializeField, Min(0f)] private float _riseSeconds = 1.6f;
        [SerializeField, Min(0f)] private float _riseDepth = 1.8f;

        private readonly List<PodiumPlaque> _names = new(PodiumRules.MAX_PLAQUES);
        private bool _dirty = true;
        private float _riseStart = -1f;

        public bool IsShown => _podium.activeSelf;
        public bool IsRising => _riseStart >= 0f;

        private void OnEnable() => _match.Changed += OnMatchChanged;
        private void OnDisable() => _match.Changed -= OnMatchChanged;
        private void OnMatchChanged() => _dirty = true;

        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                var session = _match.Session;
                var victory = session != null && session.Phase == MatchPhase.Victory;
                if (victory && !IsShown)
                    Show(session.Result);
                else if (!victory && IsShown)
                    Hide();
            }

            if (IsRising)
                Rise();
        }

        private void Show(MatchResult result)
        {
            PodiumRules.Plaques(result.Survivors, result.Eliminated, _names);
            for (var i = 0; i < _plaques.Length; i++)
            {
                var used = i < _names.Count;
                _plaques[i].SetActive(used);
                if (!used)
                    continue;
                _plaqueNames[i].text = _names[i].Name;
                _plaqueNames[i].color = _names[i].Survived ? Color.white : new Color(1f, 1f, 1f, 0.55f);
                _plaqueRenderers[i].sharedMaterial = _names[i].Survived ? _survivorMaterial : _eliminatedMaterial;
            }

            _podium.SetActive(true);
            _riseStart = Time.time;
            Rise();
        }

        private void Hide()
        {
            _podium.SetActive(false);
            _riseStart = -1f;
        }

        private void Rise()
        {
            var elapsed = Time.time - _riseStart;
            _lift.localPosition = new Vector3(0f, PodiumRules.LiftHeight(elapsed, _riseSeconds, _riseDepth), 0f);
            if (elapsed >= _riseSeconds)
                _riseStart = -1f;
        }
    }
}
