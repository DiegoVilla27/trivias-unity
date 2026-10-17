using Trivias.Shared.Characters;
using Trivias.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.Player.Editor
{
    /// <summary>
    /// Los diez personajes completos (Models/Characters/&lt;Nombre&gt;): el modelo del usuario tal cual, riggeado e
    /// importado como Humanoid (<see cref="HumanoidModels"/>). Cada uno se apunta en el catálogo de personajes.
    /// </summary>
    public static class CharacterModelBuilder
    {
        public const string FOLDER = PlayerPrefabBuilder.MODULE_ROOT + "/Models/Characters";
        /// <summary>Altura de los personajes (la de los originales, sin escalar).</summary>
        public const float HEIGHT = 1.9f;

        public static string ModelPath(string folder) => $"{FOLDER}/{folder}/{folder}_Rigged.fbx";
        public static string TexturePath(string folder) => $"{FOLDER}/{folder}/shaded.png";
        public static string MaterialPath(string folder) => $"{FOLDER}/{folder}/{folder}_Body.mat";

        /// <summary>Importa los diez como hace falta (idempotente) y los apunta en el catálogo.</summary>
        public static CharacterCatalog Build()
        {
            var catalog = SharedAssets.CharacterCatalog();
            foreach (var (id, _, folder, _) in SharedAssets.Characters)
            {
                var character = catalog.Find(id);
                if (character == null || AssetImporter.GetAtPath(ModelPath(folder)) == null)
                    continue;
                HumanoidModels.EnsureImported(ModelPath(folder), TexturePath(folder), MaterialPath(folder), $"{folder}_Body");
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(folder));
                var avatar = HumanoidModels.AvatarOf(ModelPath(folder));
                if (character.Model != model || character.Avatar != avatar)
                {
                    character.EditorSetModel(model, avatar);
                    EditorUtility.SetDirty(catalog);
                }
            }
            AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        /// <summary>El personaje por defecto (el primero de los gratuitos): con él se construye el concursante.</summary>
        public static CharacterDefinition Default(CharacterCatalog catalog) => catalog.Find(catalog.DefaultId);
    }
}
