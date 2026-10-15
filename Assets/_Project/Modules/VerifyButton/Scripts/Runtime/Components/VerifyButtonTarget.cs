using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.VerifyButton.Components
{
    /// <summary>
    /// Botón VERIFICAR (GDD 4.3): cualquier jugador cercano lo pulsa durante las apuestas y cierra todas
    /// inmediatamente. El aviso advierte del dinero que se perdería fuera de las bandejas.
    /// </summary>
    public sealed class VerifyButtonTarget : MonoBehaviour, IInteractable
    {
        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private InteractableRegistry _registry;
        [SerializeField, Min(0.5f)] private float _reach = 1.6f;
        [Tooltip("Parte que se hunde al estar cerradas las apuestas.")]
        [SerializeField] private Transform _cap;
        [SerializeField, Min(0f)] private float _pressedDepth = 0.04f;

        private string _prompt = string.Empty;
        private Vector3 _capRestPosition;

        public Vector3 Position => transform.position;
        public float Radius => _reach;

        private void Awake()
        {
            if (_cap != null)
                _capRestPosition = _cap.localPosition;
        }

        private void OnEnable()
        {
            _registry.Register(this);
            _runtime.Changed += Recalculate;
            Recalculate();
        }

        private void OnDisable()
        {
            _registry.Unregister(this);
            _runtime.Changed -= Recalculate;
        }

        public bool TryGetPrompt(in InteractionContext context, out string prompt)
        {
            prompt = _prompt;
            return prompt.Length > 0;
        }

        public void Interact(in InteractionContext context)
        {
            if (context.Action == InteractionAction.Primary)
                _runtime.Session?.RequestVerify();
        }

        private void Recalculate()
        {
            var session = _runtime.Session;
            var phase = session?.Phase ?? MatchPhase.Lobby;

            if (_cap != null)
            {
                var pressed = phase == MatchPhase.Locked || phase == MatchPhase.Reveal;
                _cap.localPosition = _capRestPosition + (pressed ? Vector3.down * _pressedDepth : Vector3.zero);
            }

            if (phase != MatchPhase.Betting)
            {
                _prompt = string.Empty;
                return;
            }

            var atRisk = session.MoneyAtRisk;
            _prompt = atRisk > 0
                ? $"E: VERIFICAR — cuidado: se perderán {MoneyFormat.Amount(atRisk)} fuera de las bandejas"
                : "E: VERIFICAR (cierra las apuestas)";
        }
    }
}
