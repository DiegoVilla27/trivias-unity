using System.Collections.Generic;
using System.Linq;
using Trivias.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.Player.Editor
{
    /// <summary>
    /// Modelo del concursante (Models/PlayerBase): el cuerpo base oficial sin ropa, pelo ni zapatos
    /// (<c>base_basic_shaded</c>) tal cual, sin perder polígonos ni altura (un millón de triángulos, 1,9 m), riggeado en
    /// Blender en pose T (<c>PlayerBase_Rigged.fbx</c>). Se ve igual que el original en Unity: color base negro y su
    /// textura <c>shaded.png</c> (con la luz ya pintada) como emisión. Sobre este cuerpo irán la ropa, el pelo y los
    /// zapatos como piezas. Se importa como Humanoid: los huesos se llaman como los de Unity y le sirven las animaciones
    /// de Starter Assets.
    /// </summary>
    public static class PlayerBaseModelBuilder
    {
        public const string FOLDER = PlayerPrefabBuilder.MODULE_ROOT + "/Models/PlayerBase";
        public const string MODEL_PATH = FOLDER + "/PlayerBase_Rigged.fbx";
        public const string BODY_MESH = "Body";
        public const string BODY_MATERIAL = "PlayerBase_Body";
        /// <summary>Altura del cuerpo, la del modelo original (no se escala).</summary>
        public const float HEIGHT = 1.9f;

        private const string COLOR_TEXTURE = FOLDER + "/shaded.png";

        private static readonly string[] HumanBones =
        {
            "Hips", "Spine", "Chest", "Neck", "Head",
            "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
            "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand",
            "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "LeftToes",
            "RightUpperLeg", "RightLowerLeg", "RightFoot", "RightToes",
        };

        public static GameObject Model() => AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH);

        public static Avatar Avatar() => AssetDatabase.LoadAllAssetsAtPath(MODEL_PATH).OfType<Avatar>().First();

        /// <summary>Deja el modelo y sus texturas importados como hacen falta (idempotente).</summary>
        public static void EnsureImported()
        {
            ConfigureTexture(COLOR_TEXTURE);

            var importer = (ModelImporter)AssetImporter.GetAtPath(MODEL_PATH);
            var dirty = false;
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                dirty = true;
            }
            // Cada material del FBX se sustituye por el nuestro del mismo nombre.
            var remaps = importer.GetExternalObjectMap();
            foreach (var (name, material) in new[] { (BODY_MATERIAL, BodyMaterial()) })
            {
                var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (!remaps.TryGetValue(key, out var current) || current != material)
                {
                    importer.AddRemap(key, material);
                    dirty = true;
                }
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
                importer.humanDescription = Describe(Model());
                importer.SaveAndReimport();
            }
        }

        /// <summary>Como lo muestra Unity al importar el original: color base negro y la textura como emisión.</summary>
        public static Material BodyMaterial() => Material(BODY_MATERIAL, COLOR_TEXTURE, Color.white);

        private static Material Material(string name, string texture, Color emission)
        {
            // La emisión es un color HDR: Unity la guarda en espacio lineal.
            var material = EditorAssets.GetOrCreateMaterial($"{FOLDER}/{name}.mat", Color.black, 0f, emission: emission.linear);
            var map = AssetDatabase.LoadAssetAtPath<Texture2D>(texture);
            material.SetTexture("_BaseMap", null);
            material.SetTexture("_EmissionMap", map);
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
