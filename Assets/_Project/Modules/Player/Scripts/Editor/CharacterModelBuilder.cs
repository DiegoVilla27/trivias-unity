using System.Collections.Generic;
using System.Linq;
using Trivias.Shared.Characters;
using Trivias.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.Player.Editor
{
    /// <summary>
    /// Los diez personajes completos (Models/Characters/&lt;Nombre&gt;): el modelo del usuario tal cual (un millón de
    /// triángulos, 1,9 m), riggeado en Blender con los huesos humanos de Unity en pose T (<c>&lt;Nombre&gt;_Rigged.fbx</c>) y su
    /// <c>shaded.png</c>. Se importan como Humanoid (les sirven las animaciones de Starter Assets) y se ven como al
    /// importar el original en Unity: color base negro y la textura (con la luz ya pintada) como emisión. Cada uno se
    /// apunta en el catálogo de personajes.
    /// </summary>
    public static class CharacterModelBuilder
    {
        public const string FOLDER = PlayerPrefabBuilder.MODULE_ROOT + "/Models/Characters";
        /// <summary>Altura de los personajes (la de los originales, sin escalar).</summary>
        public const float HEIGHT = 1.9f;

        private static readonly string[] HumanBones =
        {
            "Hips", "Spine", "Chest", "Neck", "Head",
            "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
            "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand",
            "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "LeftToes",
            "RightUpperLeg", "RightLowerLeg", "RightFoot", "RightToes",
        };

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
                EnsureImported(folder);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(folder));
                var avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath(folder)).OfType<Avatar>().FirstOrDefault();
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

        private static void EnsureImported(string folder)
        {
            ConfigureTexture(TexturePath(folder));
            var path = ModelPath(folder);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var dirty = false;
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                dirty = true;
            }
            // El material del FBX («<Nombre>_Body») se sustituye por el nuestro.
            var material = ShadedMaterial(MaterialPath(folder), TexturePath(folder));
            var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), $"{folder}_Body");
            if (!importer.GetExternalObjectMap().TryGetValue(key, out var current) || current != material)
            {
                importer.AddRemap(key, material);
                dirty = true;
            }
            if (importer.importAnimation || importer.importCameras || importer.importLights)
            {
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                dirty = true;
            }
            if (dirty)
                importer.SaveAndReimport();

            if (importer.animationType != ModelImporterAnimationType.Human || importer.humanDescription.human.Length != HumanBones.Length)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.humanDescription = Describe(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                importer.SaveAndReimport();
            }
        }

        /// <summary>Como Unity muestra los originales «shaded»: color base negro y la textura como emisión.</summary>
        private static Material ShadedMaterial(string path, string texture)
        {
            // La emisión es un color HDR: Unity la guarda en espacio lineal.
            var material = EditorAssets.GetOrCreateMaterial(path, Color.black, 0f, emission: Color.white.linear);
            material.SetTexture("_BaseMap", null);
            material.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texture));
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetFloat("_EnvironmentReflections", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Esqueleto en pose T y correspondencia directa de nombres con los huesos humanos de Unity.</summary>
        private static HumanDescription Describe(GameObject model)
        {
            var skeleton = model.GetComponentsInChildren<Transform>(true).Select(t => new SkeletonBone
            {
                name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale,
            }).ToArray();
            var human = new List<HumanBone>();
            foreach (var bone in HumanBones)
                human.Add(new HumanBone { boneName = bone, humanName = bone, limit = new HumanLimit { useDefaultValues = true } });
            return new HumanDescription
            {
                skeleton = skeleton,
                human = human.ToArray(),
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false,
            };
        }

        /// <summary>Igual que el original en Unity (2048, sRGB, comprimida).</summary>
        private static void ConfigureTexture(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null || (importer.textureType == TextureImporterType.Default && importer.sRGBTexture && importer.maxTextureSize == 2048))
                return;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
}
