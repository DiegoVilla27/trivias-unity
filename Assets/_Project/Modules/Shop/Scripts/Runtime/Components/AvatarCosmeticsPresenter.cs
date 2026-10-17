using System.Collections.Generic;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Trivias.Shop.Data;
using UnityEngine;

namespace Trivias.Shop.Components
{
    /// <summary>
    /// Viste a cada concursante con lo que lleva equipado (en todos los clientes). Solo actúa cuando cambia lo que
    /// lleva alguien (equipar ocurre en el vestíbulo, nunca durante una ronda).
    /// </summary>
    public sealed class AvatarCosmeticsPresenter : MonoBehaviour
    {
        private static readonly CosmeticSlot[] AttachedSlots =
            { CosmeticSlot.Head, CosmeticSlot.Face, CosmeticSlot.Neck, CosmeticSlot.Mouth, CosmeticSlot.Back };

        [SerializeField] private CosmeticCatalog _catalog;
        [SerializeField] private PlayerAvatarRegistry _avatars;

        private sealed class Dressed
        {
            public readonly string[] Ids = new string[CosmeticSlots.COUNT];
            public readonly GameObject[] Instances = new GameObject[CosmeticSlots.COUNT];
        }

        private readonly Dictionary<ulong, Dressed> _dressed = new();

        private void OnEnable()
        {
            _avatars.Changed += OnAvatarChanged;
            foreach (var avatar in _avatars.All)
                Apply(avatar);
        }

        private void OnDisable() => _avatars.Changed -= OnAvatarChanged;

        private void OnAvatarChanged(ulong clientId)
        {
            if (_avatars.TryGet(clientId, out var avatar))
                Apply(avatar);
            else
                _dressed.Remove(clientId); // Sus cosméticos se destruyen con el concursante.
        }

        private void Apply(IPlayerAvatar avatar)
        {
            if (!_dressed.TryGetValue(avatar.ClientId, out var dressed))
            {
                dressed = new Dressed();
                _dressed[avatar.ClientId] = dressed;
            }

            var loadout = avatar.Loadout;
            foreach (var slot in AttachedSlots)
                ApplyAttached(avatar, dressed, slot, loadout[slot]);
        }

        private void ApplyAttached(IPlayerAvatar avatar, Dressed dressed, CosmeticSlot slot, string id)
        {
            var index = (int)slot;
            var anchor = avatar.AnchorFor(slot);
            // Si cambió de personaje, sus anclajes son otros: lo puesto se vuelve a colocar en ellos.
            var instance = dressed.Instances[index];
            if (dressed.Ids[index] == id && (string.IsNullOrEmpty(id) || (instance != null && instance.transform.parent == anchor)))
                return;

            if (instance != null)
                Destroy(instance);
            dressed.Instances[index] = null;
            dressed.Ids[index] = id;

            var item = _catalog.Find(id);
            if (item?.Prefab == null || anchor == null)
                return;

            // Se engancha al anclaje de su hueco, medido en ese personaje (encima de su pelo, delante de sus ojos…): mira
            // como el concursante, va en metros y sigue las animaciones.
            instance = Instantiate(item.Prefab, anchor, false);
            instance.transform.SetLocalPositionAndRotation(item.Offset, Quaternion.identity);
            dressed.Instances[index] = instance;
        }
    }
}
