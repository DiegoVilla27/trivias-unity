using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Focos que se mueven: los de adorno de las vigas se balancean siempre un poco; en la victoria, ellos y los focos
    /// de verdad barren el plató a lo grande.
    /// </summary>
    public sealed class FixtureSway : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [Tooltip("Focos de adorno: siempre se balancean.")]
        [SerializeField] private Transform[] _fixtures = new Transform[0];
        [Tooltip("Focos con luz: solo barren en la victoria.")]
        [SerializeField] private Transform[] _partySpots = new Transform[0];

        private Quaternion[] _fixtureRest;
        private Quaternion[] _spotRest;
        private float _party;

        public bool IsPartying => _party > 0.5f;

        private void Awake()
        {
            _fixtureRest = Rest(_fixtures);
            _spotRest = Rest(_partySpots);
        }

        private void LateUpdate()
        {
            var victory = _match.Session?.Phase == MatchPhase.Victory;
            _party = Mathf.MoveTowards(_party, victory ? 1f : 0f, Time.deltaTime * 0.8f);
            var time = Time.time;
            for (var i = 0; i < _fixtures.Length; i++)
            {
                var degrees = Mathf.Lerp(StageShowRules.SwayDegrees(i, time, false), StageShowRules.SwayDegrees(i, time, true), _party);
                _fixtures[i].rotation = Quaternion.AngleAxis(degrees, Vector3.up) * _fixtureRest[i];
            }
            for (var i = 0; i < _partySpots.Length; i++)
            {
                var degrees = StageShowRules.SwayDegrees(i + 7, time, true) * _party;
                _partySpots[i].rotation = Quaternion.AngleAxis(degrees, Vector3.up) * _spotRest[i];
            }
        }

        private static Quaternion[] Rest(Transform[] transforms)
        {
            var rest = new Quaternion[transforms.Length];
            for (var i = 0; i < transforms.Length; i++)
                rest[i] = transforms[i].rotation;
            return rest;
        }
    }
}
