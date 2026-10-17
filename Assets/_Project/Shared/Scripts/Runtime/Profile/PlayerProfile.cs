using System;
using System.IO;
using Trivias.Shared.Characters;
using UnityEngine;

namespace Trivias.Shared.Profile
{
    /// <summary>
    /// Perfil del jugador local (nombre, personaje, fichas, compras y equipamiento), cargado una vez y guardado en cada cambio.
    /// Lo usan el menú (nombre), la pantalla de resultados (entregar fichas), la tienda (comprar y equipar) y el
    /// concursante (cómo se llama y lo que lleva).
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Profile/Player Profile", fileName = "PlayerProfile")]
    public sealed class PlayerProfile : ScriptableObject
    {
        private const string FILE_NAME = "trivias-profile.json";

        public event Action Changed;

        private ProfileData _data;
        private string _path;

        public ProfileData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        /// <summary>Nombre elegido (ya limpio), o vacío si no ha elegido ninguno.</summary>
        public string PlayerName => Data.name;
        /// <summary>Personaje guardado (puede estar vacío o no tenerlo ya: ver <see cref="CharacterRules.Current"/>).</summary>
        public string CharacterId => Data.character;
        public int Tokens => Data.tokens;
        public CosmeticLoadout Loadout => Data.Loadout;

        /// <summary>Cambia dónde se guarda el perfil (las pruebas usan una carpeta temporal, nunca el perfil real).</summary>
        public void UseStoragePath(string path)
        {
            _path = path;
            _data = null;
            EnsureLoaded();
            Changed?.Invoke();
        }

        /// <summary>Guarda el nombre limpio (<see cref="PlayerNameRules.Sanitize"/>). Devuelve false si no cambia.</summary>
        public bool SetName(string raw)
        {
            var name = PlayerNameRules.Sanitize(raw);
            if (name == Data.name)
                return false;
            _data.name = name;
            return Apply(true);
        }

        /// <summary>Elige personaje si lo tiene (gratuito o comprado). Devuelve false si no lo tiene o ya era ese.</summary>
        public bool TryChooseCharacter(CharacterCatalog catalog, string id)
        {
            if (catalog.Find(id) == null || !CharacterRules.IsUnlocked(id, catalog.FreeIds, Data.owned) || Data.character == id)
                return false;
            _data.character = id;
            return Apply(true);
        }

        public bool AwardMatch(string matchId, int tokens) => Apply(ProfileRules.AwardMatch(Data, matchId, tokens));

        public bool TryBuy(string itemId, int price) => Apply(ProfileRules.TryBuy(Data, itemId, price));

        public bool TryEquip(CosmeticSlot slot, string itemId) => Apply(ProfileRules.TryEquip(Data, slot, itemId));

        public void Unequip(CosmeticSlot slot)
        {
            ProfileRules.Unequip(Data, slot);
            Apply(true);
        }

        private bool Apply(bool changed)
        {
            if (!changed)
                return false;
            ProfileStore.Save(_path, _data);
            Changed?.Invoke();
            return true;
        }

        private void EnsureLoaded()
        {
            if (_data != null)
                return;
            _path ??= Path.Combine(Application.persistentDataPath, FILE_NAME);
            _data = ProfileStore.Load(_path);
        }

        private void OnEnable()
        {
            _data = null;
            _path = null;
        }
    }
}
