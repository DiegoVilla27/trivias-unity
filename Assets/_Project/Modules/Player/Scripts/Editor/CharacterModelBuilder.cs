using Trivias.Shared.Characters;
using Trivias.Shared.Profile;
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
                var anchors = MeasureAnchors(model);
                if (!SameAnchors(character, anchors))
                {
                    character.EditorSetAnchors(anchors);
                    EditorUtility.SetDirty(catalog);
                }
            }
            AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        // Alturas de referencia (todos miden 1,9 m con la misma cabeza): ojos, boca, bajo la barbilla y pecho.
        private const float EYES = 1.38f;
        private const float MOUTH = 1.22f;
        private const float NECK = 0.99f;
        private const float CHEST = 0.80f;

        /// <summary>
        /// Dónde se enganchan los cosméticos en este personaje, medido en su malla en pose T (respecto a sus pies, mirando
        /// a +Z): encima del pelo, la cara delante de los ojos, bajo la barbilla, delante de la boca y la espalda. Así unas
        /// gafas caen en los ojos de cada uno y una corona encima de su pelo, sea cual sea su peinado.
        /// </summary>
        public static Vector3[] MeasureAnchors(GameObject model)
        {
            var body = model.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var vertices = body.sharedMesh.vertices;
            var toModel = model.transform.worldToLocalMatrix * body.transform.localToWorldMatrix;
            var top = float.MinValue;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = toModel.MultiplyPoint3x4(vertices[i]);
                top = Mathf.Max(top, vertices[i].y);
            }

            // Lo más alto del pelo: el centro de lo que queda en los últimos 10 cm.
            var crown = Vector3.zero;
            var count = 0;
            float eyes = float.MinValue, mouth = float.MinValue, neck = float.MinValue, back = float.MaxValue;
            foreach (var v in vertices)
            {
                if (v.y > top - 0.1f)
                {
                    crown += v;
                    count++;
                }
                var centre = Mathf.Abs(v.x);
                // Los ojos, a los lados del centro (un mechón en medio de la frente no cuenta).
                if (centre > 0.06f && centre < 0.2f && Mathf.Abs(v.y - EYES) < 0.03f) eyes = Mathf.Max(eyes, v.z);
                if (centre < 0.08f && Mathf.Abs(v.y - MOUTH) < 0.03f) mouth = Mathf.Max(mouth, v.z);
                if (centre < 0.08f && Mathf.Abs(v.y - NECK) < 0.02f) neck = Mathf.Max(neck, v.z);
                if (centre < 0.15f && Mathf.Abs(v.y - CHEST) < 0.08f) back = Mathf.Min(back, v.z);
            }
            crown /= Mathf.Max(1, count);

            var anchors = new Vector3[CosmeticSlots.COUNT];
            anchors[(int)CosmeticSlot.Head] = new Vector3(0f, top, crown.z);
            anchors[(int)CosmeticSlot.Face] = new Vector3(0f, EYES, eyes);
            anchors[(int)CosmeticSlot.Neck] = new Vector3(0f, NECK, neck);
            anchors[(int)CosmeticSlot.Mouth] = new Vector3(0f, MOUTH, mouth);
            anchors[(int)CosmeticSlot.Back] = new Vector3(0f, CHEST, back);
            return anchors;
        }

        private static bool SameAnchors(CharacterDefinition character, Vector3[] anchors)
        {
            for (var i = 0; i < anchors.Length; i++)
            {
                if ((character.Anchor((CosmeticSlot)i) - anchors[i]).sqrMagnitude > 1e-8f)
                    return false;
            }
            return true;
        }

        /// <summary>El personaje por defecto (el primero de los gratuitos): con él se construye el concursante.</summary>
        public static CharacterDefinition Default(CharacterCatalog catalog) => catalog.Find(catalog.DefaultId);
    }
}
