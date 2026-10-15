using UnityEngine;

namespace Trivias.SelfBet.Data
{
    /// <summary>Centro de la zona de apuesta de cada cabina. Lo escribe el constructor de la escena (datos de build).</summary>
    [CreateAssetMenu(menuName = "Trivias/SelfBet/Self Bet Layout", fileName = "SelfBetLayout")]
    public sealed class SelfBetLayout : ScriptableObject
    {
        [SerializeField] private Vector3[] _zoneCenters = new Vector3[0];

        public int BoothCount => _zoneCenters.Length;
        public Vector3 ZoneCenter(int booth) => _zoneCenters[booth];

#if UNITY_EDITOR
        public void EditorSet(Vector3[] zoneCenters) => _zoneCenters = zoneCenters;
#endif
    }
}
