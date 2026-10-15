using System.Collections.Generic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Questions.Data;
using Trivias.Questions.Logic;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.Match.Network
{
    /// <summary>
    /// Historial de preguntas del grupo (CONTENIDO.md). Cada jugador apunta en su ordenador las preguntas que ve y, en
    /// la sala de espera, manda al host la lista (índices del banco). El host combina las de los participantes al
    /// empezar para elegir primero las que nadie ha visto. Todo por eventos: una vez por pregunta y una por sala.
    /// </summary>
    public sealed class QuestionHistoryNetwork : NetworkBehaviour
    {
        [SerializeField] private QuestionHistory _history;
        [SerializeField] private QuestionBank _bank;
        [SerializeField] private MatchRuntime _runtime;

        private readonly Dictionary<ulong, ushort[]> _reported = new(); // Solo en el host.
        private readonly List<ushort> _buffer = new();
        private int _recordedQuestion;
        private MatchPhase _lastPhase = MatchPhase.Lobby;

        public override void OnNetworkSpawn()
        {
            _runtime.Changed += OnMatchChanged;
            if (IsServer)
                NetworkManager.OnClientDisconnectCallback += OnClientLeft;
            else
                Report();
        }

        public override void OnNetworkDespawn()
        {
            _runtime.Changed -= OnMatchChanged;
            if (IsServer)
                NetworkManager.OnClientDisconnectCallback -= OnClientLeft;
            _reported.Clear();
        }

        /// <summary>Solo en el host: cuánto ha visto el grupo cada pregunta del banco (0 = nadie).</summary>
        public int[] GroupPenalties(IReadOnlyList<ulong> participants)
        {
            var histories = new List<IReadOnlyList<ushort>>(participants.Count);
            foreach (var client in participants)
            {
                if (client == NetworkManager.ServerClientId)
                {
                    _history.CopyIndices(_bank, _buffer);
                    histories.Add(_buffer.ToArray());
                }
                else if (_reported.TryGetValue(client, out var reported))
                {
                    histories.Add(reported);
                }
            }
            return QuestionHistoryRules.Penalties(_bank.Count, histories);
        }

        /// <summary>Solo en el host: si este cliente ya ha mandado su historial (para las pruebas y el diagnóstico).</summary>
        public bool HasReport(ulong client) => _reported.ContainsKey(client);

        private void OnMatchChanged()
        {
            var session = _runtime.Session;
            if (session == null)
                return;

            // Cada pregunta que aparece queda apuntada en el ordenador de cada jugador.
            if (session.Phase == MatchPhase.Betting && session.QuestionNumber != _recordedQuestion && session.CurrentQuestion != null)
            {
                _recordedQuestion = session.QuestionNumber;
                _history.MarkSeen(session.CurrentQuestion.Id);
            }

            // Al volver a la sala, los clientes mandan su historial al día para la próxima partida.
            if (session.Phase == MatchPhase.Lobby && _lastPhase != MatchPhase.Lobby)
            {
                _recordedQuestion = 0;
                if (!IsServer)
                    Report();
            }
            _lastPhase = session.Phase;
        }

        private void Report()
        {
            _history.CopyIndices(_bank, _buffer);
            ReportRpc(_buffer.ToArray());
        }

        [Rpc(SendTo.Server)]
        private void ReportRpc(ushort[] seen, RpcParams rpcParams = default)
        {
            // Lo que manda un cliente no se da por bueno: tamaño limitado e índices dentro del banco.
            if (seen == null)
                return;
            var count = seen.Length < QuestionHistoryRules.LIMIT ? seen.Length : QuestionHistoryRules.LIMIT;
            var valid = new List<ushort>(count);
            for (var i = seen.Length - count; i < seen.Length; i++)
            {
                if (seen[i] < _bank.Count)
                    valid.Add(seen[i]);
            }
            _reported[rpcParams.Receive.SenderClientId] = valid.ToArray();
        }

        private void OnClientLeft(ulong client) => _reported.Remove(client);
    }
}
