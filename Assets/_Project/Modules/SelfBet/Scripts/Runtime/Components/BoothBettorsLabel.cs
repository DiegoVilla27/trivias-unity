using System.Text;
using TMPro;
using Trivias.SelfBet.Data;
using Trivias.Shared.Players;
using UnityEngine;

namespace Trivias.SelfBet.Components
{
    /// <summary>
    /// Nombres de los concursantes apostados en una cabina, encima de ella (LEVEL.md). Al cerrar las apuestas dice que
    /// ya están bloqueados (boceto 05 del HUD).
    /// </summary>
    public sealed class BoothBettorsLabel : MonoBehaviour
    {
        [SerializeField] private SelfBetRuntime _runtime;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private TextMeshPro _label;
        [SerializeField, Range(0, 3)] private int _booth;

        private readonly StringBuilder _builder = new();

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            _avatars.Changed += OnAvatarChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _runtime.Changed -= Refresh;
            _avatars.Changed -= OnAvatarChanged;
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        private void Refresh()
        {
            _builder.Clear();
            var session = _runtime.Session;
            if (session != null)
            {
                var locked = false;
                foreach (var bettor in session.Book.Bettors)
                {
                    if (bettor.Booth != _booth || !bettor.IsActive)
                        continue;
                    _builder.Append(_builder.Length == 0 ? "\n" : " · ").Append(_avatars.NameOf(bettor.ClientId));
                    locked |= bettor.IsLocked;
                }
                if (_builder.Length > 0)
                    _builder.Insert(0, locked ? "APOSTADOS · BLOQUEADO" : "APOSTADOS");
            }

            var text = _builder.ToString();
            if (_label.text != text)
                _label.text = text;
        }
    }
}
