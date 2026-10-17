using System.IO;
using System.Linq;
using Trivias.Shared.Editor;
using Trivias.Shared.Profile;
using Trivias.Shop.Data;
using Trivias.Shop.Logic;
using UnityEditor;
using UnityEngine;

namespace Trivias.Shop.Editor
{
    /// <summary>Ayudas para probar la tienda en el editor (no van en el juego).</summary>
    public static class ShopTestTools
    {
        [MenuItem("Tools/Trivias/Pruebas/Desbloquear todos los accesorios")]
        public static void UnlockAllAccessories()
        {
            var path = Path.Combine(Application.persistentDataPath, PlayerProfile.FILE_NAME);
            var profile = ProfileStore.Load(path);
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(ShopBuilder.CATALOG_PATH);
            var added = ShopRules.UnlockAll(profile, catalog.Items.Select(i => i.Id));
            ProfileStore.Save(path, profile);
            // Si el perfil ya está cargado (editor abierto o jugando), que lo vuelva a leer.
            SharedAssets.PlayerProfile().UseStoragePath(path);
            Debug.Log($"[Tienda] {added} accesorios desbloqueados en {path}.");
        }

        /// <summary>Lo mismo desde la consola (Unity cerrado).</summary>
        public static void UnlockAllAccessoriesBatch()
        {
            UnlockAllAccessories();
            EditorApplication.Exit(0);
        }
    }
}
