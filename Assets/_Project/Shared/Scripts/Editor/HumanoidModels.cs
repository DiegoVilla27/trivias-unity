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

        /// <summary>
        /// Una animación hecha sobre el mismo esqueleto (FBX solo con el esqueleto animado): Humanoid con su propio avatar
        /// (las animaciones se pasan por músculos al modelo), sin moverse del sitio (la raíz se queda en la pose) y en
        /// bucle si se pide. Devuelve su clip.
        /// </summary>
        public static AnimationClip EnsureAnimationImported(string path, string modelPath, bool loop)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            // Sin fundir el nodo raíz del esqueleto (lleva un giro): el archivo debe tener la misma jerarquía que el modelo.
            if (!importer.preserveHierarchy)
            {
                importer.preserveHierarchy = true;
                importer.SaveAndReimport();
            }
            // La pose de referencia es la pose T del modelo (no el primer fotograma del archivo): si no, los músculos
            // salen desplazados y los brazos acaban en otro sitio.
            var description = DescribeAnimation(AssetDatabase.LoadAssetAtPath<GameObject>(path), AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            if (importer.animationType != ModelImporterAnimationType.Human || !SameSkeleton(importer.humanDescription, description) ||
                !importer.importAnimation)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.humanDescription = description;
                importer.SaveAndReimport();
            }

            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            var clips = importer.clipAnimations;
            var clip = clips.Length == 1 ? clips[0] : null;
            if (clip == null || clip.name != name || clip.loopTime != loop || !clip.lockRootRotation || !clip.lockRootHeightY ||
                !clip.lockRootPositionXZ || !clip.keepOriginalPositionY)
            {
                var take = importer.defaultClipAnimations[0];
                take.name = name;
                take.loopTime = loop;
                take.loopPose = loop;
                // Sin desplazarse: giro, altura y posición se quedan en la pose (así el salto se ve, pero no se va).
                take.lockRootRotation = take.lockRootHeightY = take.lockRootPositionXZ = true;
                take.keepOriginalOrientation = take.keepOriginalPositionY = take.keepOriginalPositionXZ = true;
                importer.clipAnimations = new[] { take };
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
        }

        /// <summary>
        /// Un modelo rígido del usuario (accesorio, micrófono) tal cual: su material en lugar del del FBX, sin esqueleto,
        /// animaciones, cámaras ni luces. Devuelve el modelo.
        /// </summary>
        public static GameObject EnsureStaticImported(string modelPath, string texturePath, string materialPath, string fbxMaterial)
        {
            ConfigureTexture(texturePath);
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            var dirty = false;
            if (importer.animationType != ModelImporterAnimationType.None || importer.importAnimation || importer.importCameras ||
                importer.importLights || importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
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
            if (dirty)
                importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
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

        /// <summary>
        /// Avatar de un archivo de animación con la pose T del modelo: sus mismos huesos (por nombre), con las posiciones
        /// y giros de reposo del modelo; la raíz con el nombre del archivo.
        /// </summary>
        private static HumanDescription DescribeAnimation(GameObject animation, GameObject model)
        {
            var description = Describe(model);
            var names = new HashSet<string>(animation.GetComponentsInChildren<Transform>(true).Select(t => t.name));
            var skeleton = description.skeleton.Where((bone, i) => i == 0 || names.Contains(bone.name)).ToArray();
            skeleton[0].name = animation.name;
            description.skeleton = skeleton;
            return description;
        }

        private static bool SameSkeleton(HumanDescription a, HumanDescription b)
        {
            if (a.human == null || a.human.Length != b.human.Length || a.skeleton == null || a.skeleton.Length != b.skeleton.Length)
                return false;
            for (var i = 0; i < a.skeleton.Length; i++)
            {
                if (a.skeleton[i].name != b.skeleton[i].name || Quaternion.Angle(a.skeleton[i].rotation, b.skeleton[i].rotation) > 0.01f ||
                    (a.skeleton[i].position - b.skeleton[i].position).sqrMagnitude > 1e-8f)
                    return false;
            }
            return true;
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
