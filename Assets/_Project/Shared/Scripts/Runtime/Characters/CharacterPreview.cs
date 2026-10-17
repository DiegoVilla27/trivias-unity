using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Characters
{
    /// <summary>
    /// Vista del personaje en 3D para el menú, entre el menú (pide qué personaje ver) y el expositor del concursante
    /// (lo pinta en una imagen que crea al cargar la escena). Recuerda lo último pedido, así da igual quién se active antes.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Characters/Character Preview", fileName = "CharacterPreview")]
    public sealed class CharacterPreview : ScriptableObject
    {
        public event Action<string> Requested;
        public event Action TextureChanged;
        public event Action PortraitsChanged;
        public event Action AccessoryChanged;
        public event Action<string> AccessoryPortraitRequested;

        /// <summary>Un complemento (gorra, gafas…) que se quiere ver puesto o fotografiado.</summary>
        public readonly struct Accessory
        {
            public readonly string Id;
            public readonly GameObject Prefab;
            public readonly Vector3 Offset;

            public Accessory(string id, GameObject prefab, Vector3 offset)
            {
                Id = id;
                Prefab = prefab;
                Offset = offset;
            }
        }

        private readonly Dictionary<string, Texture> _accessoryPortraits = new();
        private readonly Dictionary<string, Accessory> _pendingAccessories = new();

        /// <summary>Complemento puesto en la vista grande (sin prefab: ninguno).</summary>
        public Accessory Worn { get; private set; }

        private readonly Dictionary<string, Texture> _faces = new();
        private readonly Dictionary<string, Texture> _bodies = new();

        /// <summary>Personaje que se quiere ver, o vacío si no hay nada que mostrar (el expositor se apaga).</summary>
        public string RequestedId { get; private set; } = string.Empty;

        /// <summary>Imagen donde se pinta el personaje (la crea el expositor; nula si no hay).</summary>
        public RenderTexture Texture { get; private set; }

        public void Request(string characterId)
        {
            characterId ??= string.Empty;
            if (characterId == RequestedId)
                return;
            RequestedId = characterId;
            Requested?.Invoke(characterId);
        }

        public void SetTexture(RenderTexture texture)
        {
            Texture = texture;
            TextureChanged?.Invoke();
        }

        /// <summary>Retrato de la cara del personaje (lista de jugadores), o nulo si aún no está hecho.</summary>
        public Texture Face(string characterId) =>
            !string.IsNullOrEmpty(characterId) && _faces.TryGetValue(characterId, out var face) ? face : null;

        /// <summary>Foto de cuerpo entero del personaje (tarjetas de la tienda), o nula si aún no está hecha.</summary>
        public Texture Body(string characterId) =>
            !string.IsNullOrEmpty(characterId) && _bodies.TryGetValue(characterId, out var body) ? body : null;

        /// <summary>Pone (o quita, con prefab nulo) un complemento al personaje de la vista grande.</summary>
        public void Wear(string id, GameObject prefab, Vector3 offset)
        {
            id ??= string.Empty;
            if (id == (Worn.Id ?? string.Empty))
                return;
            Worn = new Accessory(id, prefab, offset);
            AccessoryChanged?.Invoke();
        }

        /// <summary>Foto de la cabeza con el complemento puesto (tarjetas de la tienda), o nula si aún no está hecha.</summary>
        public Texture AccessoryPortrait(string id) =>
            !string.IsNullOrEmpty(id) && _accessoryPortraits.TryGetValue(id, out var portrait) ? portrait : null;

        /// <summary>Pide la foto de un complemento; el expositor la hace en cuanto puede.</summary>
        public void RequestAccessoryPortrait(string id, GameObject prefab, Vector3 offset)
        {
            if (string.IsNullOrEmpty(id) || _accessoryPortraits.ContainsKey(id))
                return;
            _pendingAccessories[id] = new Accessory(id, prefab, offset);
            AccessoryPortraitRequested?.Invoke(id);
        }

        /// <summary>Fotos pedidas que aún no se han hecho (el expositor las hace al activarse).</summary>
        public IReadOnlyDictionary<string, Accessory> PendingAccessoryPortraits => _pendingAccessories;

        public void SetAccessoryPortrait(string id, Texture portrait)
        {
            _pendingAccessories.Remove(id);
            _accessoryPortraits[id] = portrait;
            PortraitsChanged?.Invoke();
        }

        /// <summary>Lo llama el expositor al terminar los retratos (al cargar la escena).</summary>
        public void SetPortraits(IReadOnlyDictionary<string, Texture> faces, IReadOnlyDictionary<string, Texture> bodies)
        {
            _faces.Clear();
            _bodies.Clear();
            if (faces != null)
                foreach (var pair in faces)
                    _faces[pair.Key] = pair.Value;
            if (bodies != null)
                foreach (var pair in bodies)
                    _bodies[pair.Key] = pair.Value;
            PortraitsChanged?.Invoke();
        }

        private void OnEnable()
        {
            RequestedId = string.Empty;
            Texture = null;
            _faces.Clear();
            _bodies.Clear();
            _accessoryPortraits.Clear();
            _pendingAccessories.Clear();
            Worn = default;
        }
    }
}
