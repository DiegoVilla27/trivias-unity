using UnityEngine;

namespace Trivias.Shared.Spawning
{
    /// <summary>
    /// Puntos de aparición de jugadores. Los escribe el constructor del plató en tiempo de edición;
    /// en runtime solo se leen (sin buscar objetos en la escena).
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Spawning/Spawn Point Set", fileName = "SpawnPointSet")]
    public sealed class SpawnPointSet : ScriptableObject
    {
        [SerializeField] private Vector3[] _positions = { Vector3.zero };
        [SerializeField] private float[] _yawDegrees = { 0f };

        public int Count => _positions.Length;

        public void Get(int index, out Vector3 position, out Quaternion rotation)
        {
            var i = Mathf.Abs(index) % _positions.Length;
            position = _positions[i];
            rotation = Quaternion.Euler(0f, i < _yawDegrees.Length ? _yawDegrees[i] : 0f, 0f);
        }

#if UNITY_EDITOR
        public void EditorSet(Vector3[] positions, float[] yawDegrees)
        {
            _positions = positions;
            _yawDegrees = yawDegrees;
        }
#endif
    }
}
