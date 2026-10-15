using System;
using System.IO;
using UnityEngine;

namespace Trivias.Shared.Profile
{
    /// <summary>
    /// Perfil del jugador local (fichas, compras y equipamiento), cargado una vez y guardado en cada cambio.
    /// Lo usan la pantalla de resultados (entregar fichas), la tienda (comprar y equipar) y el concursante (lo que lleva).
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
