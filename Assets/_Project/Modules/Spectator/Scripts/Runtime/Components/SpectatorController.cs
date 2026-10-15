using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Players;
using Trivias.Spectator.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Spectator.Components
{
    /// <summary>
    /// Cámara de espectador del eliminado (GDD 5.3): tras ver su expulsión, mira la partida por encima del hombro de un
    /// concursante activo. A/D o flechas cambian de concursante; Espacio alterna con la vista general del plató. Si el
    /// que mira cae o se va, pasa solo al siguiente. Al volver a la sala recupera su concursante.
    /// No conoce Player ni Camera: lee los concursantes del registro y mueve la cámara por el mismo canal que el
    /// concursante local. Funciona por eventos; Update solo espera el arranque tras la expulsión.
    /// </summary>
    public sealed class SpectatorController : MonoBehaviour
    {
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private TransformEventChannel _cameraTarget;
        [SerializeField] private PlayerControlGate _controlGate;
        [SerializeField] private GameObject _banner;
        [SerializeField] private TextMeshProUGUI _bannerText;
        [Tooltip("Segundos tras la expulsión antes de pasar a la vista de espectador (para ver la expulsión).")]
        [SerializeField, Min(0f)] private float _startDelay = 2.5f;

        private readonly List<ulong> _candidates = new();
        private InputAction _next;
        private InputAction _previous;
        private InputAction _overview;
        private float _startAt = -1f;
        private bool _hasFollowed;
        private ulong _followed;

        public bool IsSpectating { get; private set; }
        public bool IsWaitingToStart => _startAt >= 0f;
        public bool IsOverview => IsSpectating && !_hasFollowed;
        public bool IsFollowing(ulong clientId) => IsSpectating && _hasFollowed && _followed == clientId;
        public bool IsBannerShown => _banner.activeSelf;
        public string BannerText => _bannerText.text;

        private void Awake()
        {
            _next = new InputAction("SpectateNext", InputActionType.Button);
            _next.AddBinding("<Keyboard>/d");
            _next.AddBinding("<Keyboard>/rightArrow");
            _previous = new InputAction("SpectatePrevious", InputActionType.Button);
            _previous.AddBinding("<Keyboard>/a");
            _previous.AddBinding("<Keyboard>/leftArrow");
            _overview = new InputAction("SpectateOverview", InputActionType.Button, "<Keyboard>/space");
            _banner.SetActive(false);
        }

        private void OnEnable()
        {
            _avatars.Changed += OnAvatarChanged;
            _controlGate.Changed += OnControlGateChanged;
            _next.performed += OnNextPerformed;
            _previous.performed += OnPreviousPerformed;
            _overview.performed += OnOverviewPerformed;
            _next.Enable();
            _previous.Enable();
            _overview.Enable();
        }

        private void OnDisable()
        {
            _avatars.Changed -= OnAvatarChanged;
            _controlGate.Changed -= OnControlGateChanged;
            _next.performed -= OnNextPerformed;
            _previous.performed -= OnPreviousPerformed;
            _overview.performed -= OnOverviewPerformed;
            _next.Disable();
            _previous.Disable();
            _overview.Disable();
        }

        private void OnDestroy()
        {
            _next.Dispose();
            _previous.Dispose();
            _overview.Dispose();
        }

        private void Update()
        {
            if (_startAt < 0f || Time.time < _startAt)
                return;
            _startAt = -1f;
            // Si la partida ya terminó (expulsado en la última pregunta), se queda en la vista general de la celebración.
            if (_controlGate.MatchAllowsMovement)
                Begin();
        }

        // ---- Acciones (públicas para las pruebas) ----

        public void Next() => Step(+1);

        public void Previous() => Step(-1);

        /// <summary>Alterna entre la vista general y seguir a un concursante.</summary>
        public void ToggleOverview()
        {
            if (!IsSpectating)
                return;
            if (_hasFollowed)
                ShowOverview();
            else
                Step(0);
        }

        // ---- Estado ----

        private void OnAvatarChanged(ulong clientId)
        {
            var local = FindLocal();
            var eliminated = local != null && local.IsEliminated;
            if (!eliminated)
            {
                // Sin concursante local (fuera de la sala) o de vuelta en juego: el propio concursante ya movió la cámara.
                Stop();
                return;
            }

            if (!IsSpectating)
            {
                if (_startAt < 0f)
                    _startAt = Time.time + _startDelay;
                return;
            }

            // El que miramos cayó o se fue: al siguiente. Si cambió otra cosa (nombre...), solo se redibuja el rótulo.
            if (_hasFollowed && (!_avatars.TryGet(_followed, out var followed) || followed.IsEliminated))
                Step(+1);
            else
                RefreshBanner();
        }

        private void OnControlGateChanged(bool _) => RefreshBanner();

        private void Begin()
        {
            IsSpectating = true;
            _hasFollowed = false;
            Step(0);
        }

        private void Stop()
        {
            _startAt = -1f;
            IsSpectating = false;
            _hasFollowed = false;
            _banner.SetActive(false);
        }

        private void Step(int step)
        {
            if (!IsSpectating)
                return;
            CollectCandidates();
            var index = SpectatorTargets.Step(_candidates, _hasFollowed, _followed, step);
            if (index == SpectatorTargets.NONE || !_avatars.TryGet(_candidates[index], out var avatar))
            {
                ShowOverview();
                return;
            }

            _hasFollowed = true;
            _followed = avatar.ClientId;
            _cameraTarget.Raise(avatar.ViewTarget);
            RefreshBanner();
        }

        private void ShowOverview()
        {
            _hasFollowed = false;
            _cameraTarget.Raise(null);
            RefreshBanner();
        }

        private void RefreshBanner()
        {
            // Solo mientras se juega: en los resultados y en la sala el rótulo sobra.
            var visible = IsSpectating && _controlGate.MatchAllowsMovement;
            _banner.SetActive(visible);
            if (!visible)
                return;
            CollectCandidates();
            var name = _hasFollowed ? _avatars.NameOf(_followed) : string.Empty;
            _bannerText.text = SpectatorText.Banner(name, _candidates.Count > 0);
        }

        private void CollectCandidates()
        {
            _candidates.Clear();
            foreach (var avatar in _avatars.All)
            {
                if (!avatar.IsEliminated && !avatar.IsLocal)
                    _candidates.Add(avatar.ClientId);
            }
            _candidates.Sort();
        }

        private IPlayerAvatar FindLocal()
        {
            foreach (var avatar in _avatars.All)
            {
                if (avatar.IsLocal)
                    return avatar;
            }
            return null;
        }

        private bool AcceptsInput => IsSpectating && !_controlGate.IsPaused;

        private void OnNextPerformed(InputAction.CallbackContext _)
        {
            if (AcceptsInput)
                Next();
        }

        private void OnPreviousPerformed(InputAction.CallbackContext _)
        {
            if (AcceptsInput)
                Previous();
        }

        private void OnOverviewPerformed(InputAction.CallbackContext _)
        {
            if (AcceptsInput)
                ToggleOverview();
        }
    }
}
