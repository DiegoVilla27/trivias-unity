using System;
using System.Collections.Generic;
using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Shop.Data
{
    /// <summary>Un cosmético de la tienda (GDD 11): estético, sin ventajas, compra permanente.</summary>
    [Serializable]
    public sealed class CosmeticItem
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private CosmeticSlot _slot;
        [SerializeField, Min(0)] private int _price;
        [Tooltip("Objeto que se coloca en la cabeza (gorras, gafas...). Vacío para los colores de traje.")]
        [SerializeField] private GameObject _prefab;
        [Tooltip("Posición respecto a la cabeza, en el espacio del concursante.")]
        [SerializeField] private Vector3 _offset;
        [Tooltip("Color del traje (solo para el hueco Color).")]
        [SerializeField] private Color _tint = Color.white;

        public CosmeticItem(string id, string displayName, CosmeticSlot slot, int price, GameObject prefab, Vector3 offset, Color tint)
        {
            _id = id;
            _displayName = displayName;
            _slot = slot;
            _price = price;
            _prefab = prefab;
            _offset = offset;
            _tint = tint;
        }

        public string Id => _id;
        public string DisplayName => _displayName;
        public CosmeticSlot Slot => _slot;
        public int Price => _price;
        public GameObject Prefab => _prefab;
        public Vector3 Offset => _offset;
        public Color Tint => _tint;
    }

    /// <summary>Catálogo de la tienda, construido en tiempo de edición.</summary>
    [CreateAssetMenu(menuName = "Trivias/Shop/Cosmetic Catalog", fileName = "CosmeticCatalog")]
    public sealed class CosmeticCatalog : ScriptableObject
    {
        [SerializeField] private List<CosmeticItem> _items = new();

        public IReadOnlyList<CosmeticItem> Items => _items;

        public CosmeticItem Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            foreach (var item in _items)
            {
                if (item.Id == id)
                    return item;
            }
            return null;
        }

#if UNITY_EDITOR
        public void EditorSet(List<CosmeticItem> items) => _items = items;
#endif
    }
}
