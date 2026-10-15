using Trivias.SelfBet.Data;
using Trivias.SelfBet.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.SelfBet.Components
{
    /// <summary>
    /// Zona de apuesta delante de una cabina: dentro de ella E apuesta al concursante, cambia la apuesta a esta cabina
    /// o la cancela si ya estaba aquí. Si todos los activos están apostados (nadie libre para ir al botón),
    /// Q verifica desde aquí. El aviso se recalcula solo cuando cambian los comodines.
    /// </summary>
    public sealed class BetZoneTarget : MonoBehaviour, IInteractable
    {
        [SerializeField] private SelfBetConfig _config;
        [SerializeField] private SelfBetRuntime _runtime;
        [SerializeField] private InteractableRegistry _registry;
        [SerializeField, Range(0, 3)] private int _booth;
        [SerializeField] private VoidEventChannel _verifyFromZoneRequested;

        private string _prompt = string.Empty;

        public Vector3 Position => transform.position;
        public float Radius => _config.ZoneRadius;

        private char Letter => (char)('A' + _booth);

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
            var session = _runtime.Session;
            if (session == null)
                return;

            if (context.Action == InteractionAction.Primary)
                session.RequestToggle(_booth);
            else if (context.Action == InteractionAction.Secondary && CanVerifyFromHere(session))
                _verifyFromZoneRequested.Raise();
        }

        private bool CanVerifyFromHere(ISelfBetSession session) =>
            session.IsOpen && session.Book.HasBet(session.LocalClientId) &&
            session.Book.Bettors[session.Book.IndexOf(session.LocalClientId)].Booth == _booth &&
            session.Book.AllActiveHaveBet;

        private void Recalculate()
        {
            var session = _runtime.Session;
            if (session == null || !session.IsOpen)
            {
                _prompt = string.Empty;
                return;
            }

            var book = session.Book;
            var index = book.IndexOf(session.LocalClientId);
            if (index < 0 || book.Bettors[index].IsEliminated)
            {
                _prompt = string.Empty;
                return;
            }

            var bettor = book.Bettors[index];
            if (bettor.Booth == _booth)
            {
                _prompt = CanVerifyFromHere(session)
                    ? $"E: cancelar mi apuesta en {Letter}     Q: VERIFICAR (estáis todos apostados)"
                    : $"E: cancelar mi apuesta en {Letter}";
                return;
            }

            if (bettor.HasBet)
            {
                _prompt = $"E: cambiar mi apuesta a {Letter}";
                return;
            }

            var uses = book.AvailableUses(session.LocalClientId);
            _prompt = uses > 0
                ? bettor.PersonalUses > 0
                    ? $"E: apostarme en {Letter} (comodines: {uses})"
                    : $"E: apostarme en {Letter} (reserva del equipo: {uses})"
                : "Sin comodines disponibles";
        }
    }
}
