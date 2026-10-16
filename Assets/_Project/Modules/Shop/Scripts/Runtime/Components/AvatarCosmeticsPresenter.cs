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
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly CosmeticSlot[] AttachedSlots = { CosmeticSlot.Head, CosmeticSlot.Face, CosmeticSlot.Neck };

        [SerializeField] private CosmeticCatalog _catalog;
        [SerializeField] private PlayerAvatarRegistry _avatars;

        private sealed class Dressed
        {
            public readonly string[] Ids = new string[CosmeticSlots.COUNT];
            public readonly GameObject[] Instances = new GameObject[CosmeticSlots.COUNT];
        }

        private readonly Dictionary<ulong, Dressed> _dressed = new();
        private MaterialPropertyBlock _block;

        private void OnEnable()
        {
            _block ??= new MaterialPropertyBlock();
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
            ApplyColor(avatar, dressed, loadout.Color);
        }

        private void ApplyAttached(IPlayerAvatar avatar, Dressed dressed, CosmeticSlot slot, string id)
        {
            var index = (int)slot;
            if (dressed.Ids[index] == id && (string.IsNullOrEmpty(id) || dressed.Instances[index] != null))
                return;

            if (dressed.Instances[index] != null)
                Destroy(dressed.Instances[index]);
            dressed.Instances[index] = null;
            dressed.Ids[index] = id;

            var item = _catalog.Find(id);
            if (item?.Prefab == null || avatar.HeadAnchor == null)
                return;

            // Se coloca alineado con el concursante y luego se engancha a la cabeza para seguir sus animaciones.
            var root = avatar.HeadAnchor.root;
            var instance = Instantiate(item.Prefab);
            instance.transform.SetPositionAndRotation(avatar.HeadAnchor.position + root.rotation * item.Offset, root.rotation);
            instance.transform.SetParent(avatar.HeadAnchor, true);
            dressed.Instances[index] = instance;
        }

        private void ApplyColor(IPlayerAvatar avatar, Dressed dressed, string id)
        {
            var index = (int)CosmeticSlot.Color;
            if (dressed.Ids[index] == id)
                return;
            dressed.Ids[index] = id;

            var item = _catalog.Find(id);
            foreach (var body in avatar.BodyRenderers)
            {
                if (body == null)
                    continue;
                if (item == null)
                {
                    body.SetPropertyBlock(null);
                    continue;
                }
                body.GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, item.Tint);
                body.SetPropertyBlock(_block);
            }
        }
    }
}
