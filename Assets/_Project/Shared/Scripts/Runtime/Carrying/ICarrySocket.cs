using UnityEngine;

namespace Trivias.Shared.Carrying
{
    /// <summary>Punto donde un concursante sujeta el objeto que transporta. Lo implementa el módulo Player.</summary>
    public interface ICarrySocket
    {
        Transform CarryPoint { get; }
    }
}
