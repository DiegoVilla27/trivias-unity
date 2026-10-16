using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Cámaras de estudio que siguen la acción: miran la mesa mientras se apuesta, el botón al verificar, la cabina
    /// correcta al revelar y vuelven a la mesa en la victoria; con un leve vaivén de operador.
    /// </summary>
    public sealed class StudioCameraOperators : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private Transform[] _cameras = new Transform[0];
        [SerializeField] private Transform _table;
        [SerializeField] private Transform _verifyButton;
        [Tooltip("Cabinas A..D, para mirar la correcta al revelar.")]
        [SerializeField] private Transform[] _booths = new Transform[0];
        [SerializeField, Min(0.05f)] private float _turnSeconds = 0.9f;

        private float[] _yaw;
        private float[] _yawVelocity;

        public Vector3 Focus { get; private set; }

        private void Awake()
        {
            _yaw = new float[_cameras.Length];
            _yawVelocity = new float[_cameras.Length];
            for (var i = 0; i < _cameras.Length; i++)
                _yaw[i] = _cameras[i].eulerAngles.y;
        }

        private void LateUpdate()
        {
            var session = _match.Session;
            Focus = FocusFor(session);
            var time = Time.time;
            for (var i = 0; i < _cameras.Length; i++)
            {
                var camera = _cameras[i];
                var toFocus = Focus - camera.position;
                var target = Mathf.Atan2(toFocus.x, toFocus.z) * Mathf.Rad2Deg + StageShowRules.SwayDegrees(i, time, false) * 0.4f;
                _yaw[i] = Mathf.SmoothDampAngle(_yaw[i], target, ref _yawVelocity[i], _turnSeconds);
                camera.rotation = Quaternion.Euler(0f, _yaw[i], 0f);
            }
        }

        private Vector3 FocusFor(IMatchSession session)
        {
            if (session == null)
                return _table.position;
            switch (session.Phase)
            {
                case MatchPhase.Locked:
                    return _verifyButton.position;
                case MatchPhase.Reveal:
                    var correct = session.CorrectBooth;
                    return correct >= 0 && correct < _booths.Length ? _booths[correct].position : _table.position;
                default:
                    return _table.position;
            }
        }
    }
}
