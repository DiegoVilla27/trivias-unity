using Unity.Netcode.Components;

namespace Trivias.Player.Network
{
    /// <summary>El dueño del personaje manda el estado de su Animator (igual que su posición).</summary>
    public sealed class OwnerNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
