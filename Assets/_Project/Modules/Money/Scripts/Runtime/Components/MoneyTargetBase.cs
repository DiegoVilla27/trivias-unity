using TMPro;
using Trivias.Money.Data;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Base de mesa y bandejas: se registran como interactivos y recalculan aviso y total solo cuando cambia
    /// el dinero (nunca por frame). El aviso se guarda ya formateado para no crear textos en cada consulta.
    /// </summary>
    public abstract class MoneyTargetBase : MonoBehaviour, IInteractable
    {
        [SerializeField] protected MoneyConfig _config;
        [SerializeField] private MoneyRuntime _runtime;
        [SerializeField] private InteractableRegistry _registry;
        [SerializeField] private TextMeshPro _totalLabel;

        private string _prompt = string.Empty;

        public Vector3 Position => transform.position;
        public abstract float Radius { get; }

        protected IMoneySession Session => _runtime.Session;

        private void OnEnable()
        {
            _registry.Register(this);
            _runtime.Changed += OnMoneyChanged;
            Recalculate();
        }

        private void OnDisable()
        {
            _registry.Unregister(this);
            _runtime.Changed -= OnMoneyChanged;
        }

        public bool TryGetPrompt(in InteractionContext context, out string prompt)
        {
            prompt = _prompt;
            return prompt.Length > 0;
        }

        public void Interact(in InteractionContext context)
        {
            if (Session != null)
                SendRequest(Session, context.Action);
        }

        protected abstract void SendRequest(IMoneySession session, InteractionAction action);

        protected const string SEPARATOR = "     ";

        /// <summary>Devuelve el aviso y el texto del total para el estado actual.</summary>
        protected abstract void Describe(IMoneySession session, out string prompt, out string totalLabel);

        private void OnMoneyChanged(int _) => Recalculate();

        private void Recalculate()
        {
            var session = Session;
            if (session == null)
            {
                _prompt = string.Empty;
                SetLabel(string.Empty);
                return;
            }

            Describe(session, out _prompt, out var label);
            if (session.IsLocked)
                _prompt = string.Empty; // Apuestas cerradas: el total se sigue viendo, pero no se puede interactuar.
            SetLabel(label);
        }

        private void SetLabel(string text)
        {
            if (_totalLabel != null && _totalLabel.text != text)
                _totalLabel.text = text;
        }
    }
}
