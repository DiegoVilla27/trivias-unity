using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Input
{
    /// <summary>Paneles de interfaz activos sobre los que un clic no captura el ratón.</summary>
    [CreateAssetMenu(menuName = "Trivias/Input/Cursor Capture Blockers", fileName = "CursorCaptureBlockers")]
    public sealed class CursorCaptureBlockers : ScriptableObject
    {
        private readonly List<ICursorCaptureBlocker> _blockers = new();

        public void Register(ICursorCaptureBlocker blocker)
        {
            if (!_blockers.Contains(blocker))
                _blockers.Add(blocker);
        }

        public void Unregister(ICursorCaptureBlocker blocker) => _blockers.Remove(blocker);

        public bool IsBlocked(Vector2 screenPosition)
        {
            for (var i = 0; i < _blockers.Count; i++)
            {
                if (_blockers[i].BlocksAt(screenPosition))
                    return true;
            }
            return false;
        }

        private void OnEnable() => _blockers.Clear();
    }
}
