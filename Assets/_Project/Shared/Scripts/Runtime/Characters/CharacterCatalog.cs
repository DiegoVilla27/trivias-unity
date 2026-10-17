using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Characters
{
    /// <summary>
    /// Un personaje que se puede elegir: su modelo completo (tal cual, riggeado con los huesos humanos de Unity), su
    /// avatar Humanoid y un color que lo representa (p. ej. su nombre en la sala de espera).
    /// </summary>
    [Serializable]
    public sealed class CharacterDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [Tooltip("Color que lo representa (nombre en la sala de espera).")]
        [SerializeField] private Color _tint = Color.white;
        [Tooltip("Modelo riggeado (FBX importado como Humanoid).")]
        [SerializeField] private GameObject _model;
        [SerializeField] private Avatar _avatar;

        public CharacterDefinition(string id, string displayName, Color tint)
        {
            _id = id;
            _displayName = displayName;
            _tint = tint;
        }

        public string Id => _id;
        public string DisplayName => _displayName;
        public Color Tint => _tint;
        public GameObject Model => _model;
        public Avatar Avatar => _avatar;

#if UNITY_EDITOR
        public void EditorSetModel(GameObject model, Avatar avatar)
        {
            _model = model;
            _avatar = avatar;
        }
#endif
    }

    /// <summary>
    /// Personajes del juego (GDD 10): unos gratis desde el principio y el resto se compran en la tienda. Dos jugadores de
    /// la misma sala nunca llevan el mismo. Los gratuitos se eligen en <c>Free Character Ids</c> (inspector).
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Characters/Character Catalog", fileName = "CharacterCatalog")]
    public sealed class CharacterCatalog : ScriptableObject
    {
        [SerializeField] private List<CharacterDefinition> _characters = new();
        [Tooltip("Los personajes gratuitos, disponibles para todos desde el principio. El resto se compran en la tienda.")]
        [SerializeField] private string[] _freeCharacterIds = new string[0];
        [Tooltip("Fichas que cuesta cada personaje de pago.")]
        [SerializeField, Min(0)] private int _price = 1200;

        private readonly List<string> _ids = new();

        public IReadOnlyList<CharacterDefinition> Characters => _characters;
        public IReadOnlyList<string> FreeIds => _freeCharacterIds;
        public int Price => _price;

        /// <summary>Ids en el orden del catálogo.</summary>
        public IReadOnlyList<string> Ids
        {
            get
            {
                if (_ids.Count != _characters.Count)
                {
                    _ids.Clear();
                    foreach (var character in _characters)
                        _ids.Add(character.Id);
                }
                return _ids;
            }
        }

        public CharacterDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            foreach (var character in _characters)
            {
                if (character.Id == id)
                    return character;
            }
            return null;
        }

        public bool IsFree(string id) => Array.IndexOf(_freeCharacterIds, id) >= 0;

        /// <summary>El que lleva quien no ha elegido ninguno: el primero de los gratuitos.</summary>
        public string DefaultId => _freeCharacterIds.Length > 0 ? _freeCharacterIds[0] : _characters.Count > 0 ? _characters[0].Id : string.Empty;

        private void OnEnable() => _ids.Clear();

#if UNITY_EDITOR
        public void EditorSet(List<CharacterDefinition> characters, string[] freeIds, int price)
        {
            _characters = characters;
            _freeCharacterIds = freeIds;
            _price = price;
            _ids.Clear();
        }
#endif
    }
}
