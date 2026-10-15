using System;
using Unity.Collections;
using Unity.Netcode;

namespace Trivias.Match.Network
{
    /// <summary>Resultado final de la partida (GDD 3.3), igual para todos los jugadores.</summary>
    public struct MatchResultNet : INetworkSerializable, IEquatable<MatchResultNet>
    {
        public uint MatchId;
        public bool Victory;
        public byte QuestionsPassed;
        public long FinalMoney;
        public float DurationSeconds;
        public short TokensEach;
        public FixedString128Bytes Survivors;
        public FixedString128Bytes Eliminated;
        public byte StartingPlayers;
        public bool StartedAtFirstQuestion;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MatchId);
            serializer.SerializeValue(ref Victory);
            serializer.SerializeValue(ref QuestionsPassed);
            serializer.SerializeValue(ref FinalMoney);
            serializer.SerializeValue(ref DurationSeconds);
            serializer.SerializeValue(ref TokensEach);
            serializer.SerializeValue(ref Survivors);
            serializer.SerializeValue(ref Eliminated);
            serializer.SerializeValue(ref StartingPlayers);
            serializer.SerializeValue(ref StartedAtFirstQuestion);
        }

        public bool Equals(MatchResultNet other) =>
            MatchId == other.MatchId && Victory == other.Victory && QuestionsPassed == other.QuestionsPassed && FinalMoney == other.FinalMoney &&
            DurationSeconds.Equals(other.DurationSeconds) && TokensEach == other.TokensEach &&
            Survivors.Equals(other.Survivors) && Eliminated.Equals(other.Eliminated) &&
            StartingPlayers == other.StartingPlayers && StartedAtFirstQuestion == other.StartedAtFirstQuestion;

        public override bool Equals(object obj) => obj is MatchResultNet other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(HashCode.Combine(MatchId, Victory, QuestionsPassed, FinalMoney, DurationSeconds, TokensEach, Survivors,
                Eliminated), StartingPlayers, StartedAtFirstQuestion);
    }
}
