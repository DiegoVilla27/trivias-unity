using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Importa los modelos completos del usuario (concursantes y presentador) tal cual: un millón de triángulos y su
    /// altura original, riggeados en Blender con los huesos humanos de Unity en pose T (<c>&lt;Nombre&gt;_Rigged.fbx</c>)
    /// y su <c>shaded.png</c>. Quedan como Humanoid (les sirven las animaciones de Starter Assets) y se ven como al
    /// importar el original en Unity: color base negro y la textura (con la luz ya pintada) como emisión.
    /// </summary>
    public static class HumanoidModels
    {
        public static readonly string[] HumanBones =
        {
            "Hips", "Spine", "Chest", "Neck", "Head",
            "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
            "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand",
            "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "LeftToes",
            "RightUpperLeg", "RightLowerLeg", "RightFoot", "RightToes",
        };

        /// <summary>
        /// Deja el modelo como hace falta (idempotente): su material en lugar del del FBX (<paramref name="fbxMaterial"/>),
        /// sin animaciones, cámaras ni luces, y Humanoid con su esqueleto.
        /// </summary>
        public static void EnsureImported(string modelPath, string texturePath, string materialPath, string fbxMaterial)
        {
            ConfigureTexture(texturePath);
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            var dirty = false;
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                dirty = true;
            }
            var material = ShadedMaterial(materialPath, texturePath);
            var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), fbxMaterial);
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
                importer.humanDescription = Describe(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
                importer.SaveAndReimport();
            }
        }

        public static Avatar AvatarOf(string modelPath) =>
            AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();

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
