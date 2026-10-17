using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Tipografías del plató (licencia OFL, en <c>Shared/UI/Fonts</c>):
    /// <list type="bullet">
    /// <item><b>Display</b>: Lilita One, rotulación redonda de concurso para titulares (logo, cabecera, carteles).</item>
    /// <item><b>Body</b>: Poppins SemiBold, moderna y muy legible a distancia; su negrita es Poppins ExtraBold real.</item>
    /// </list>
    /// Las tablas de caracteres se generan una vez en edición (estáticas, con todo el español) y no cambian al jugar.
    /// </summary>
    public static class StageFonts
    {
        private const string FONTS = "Assets/_Project/Shared/UI/Fonts";
        private const int SAMPLING_SIZE = 90;
        private const int PADDING = 9;
        private const int ATLAS_SIZE = 2048;
        private const int BOLD_WEIGHT_INDEX = 7;

        /// <summary>Latin-1 completo (español con tildes, ñ, ¿, ¡, º) y la puntuación tipográfica que usa el juego.</summary>
        private static string Characters
        {
            get
            {
                var characters = new System.Text.StringBuilder();
                for (var c = 32; c < 127; c++)
                    characters.Append((char)c);
                for (var c = 160; c < 256; c++)
                    characters.Append((char)c);
                characters.Append("–—‘’“”‚„…€•™");
                return characters.ToString();
            }
        }

        public static TMP_FontAsset Display() => GetOrCreate("LilitaOne-Regular", null);

        /// <summary>
        /// Lilita One para rótulos grandes de la interfaz, como los títulos de los menús: contorno azul noche grueso y
        /// sombra suave debajo. El degradado de color lo pone cada texto.
        /// </summary>
        public static Material DisplayTitle()
        {
            var font = Display();
            var path = $"{FONTS}/LilitaOne-Regular Title.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material) { name = "LilitaOne-Regular Title" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = font.material.shader;
            material.SetTexture("_MainTex", font.atlasTexture);
            material.EnableKeyword("OUTLINE_ON");
            material.SetFloat("_OutlineWidth", 0.24f);
            material.SetColor("_OutlineColor", new Color(0.04f, 0.07f, 0.15f));
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.55f));
            material.SetFloat("_UnderlayOffsetY", -0.9f);
            material.SetFloat("_UnderlaySoftness", 0.3f);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static TMP_FontAsset Body() => GetOrCreate("Poppins-SemiBold", GetOrCreate("Poppins-ExtraBold", null));

        private static TMP_FontAsset GetOrCreate(string fontName, TMP_FontAsset bold)
        {
            var path = $"{FONTS}/{fontName} SDF.asset";
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset == null)
                asset = Create(fontName, path);

            // Si falta algún carácter raro, TMP lo busca en la fuente por defecto en vez de dibujar un cuadrado.
            var fallback = TMP_Settings.defaultFontAsset;
            if (fallback != null && fallback != asset && (asset.fallbackFontAssetTable == null || !asset.fallbackFontAssetTable.Contains(fallback)))
            {
                asset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
                EditorUtility.SetDirty(asset);
            }

            if (bold != null)
            {
                var serialized = new SerializedObject(asset);
                var boldSlot = serialized.FindProperty($"m_FontWeightTable.Array.data[{BOLD_WEIGHT_INDEX}].regularTypeface");
                if (boldSlot != null && boldSlot.objectReferenceValue != bold)
                {
                    boldSlot.objectReferenceValue = bold;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            return asset;
        }

        private static TMP_FontAsset Create(string fontName, string path)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>($"{FONTS}/{fontName}.ttf");
            if (font == null)
                throw new System.InvalidOperationException($"No se encuentra la fuente {FONTS}/{fontName}.ttf.");

            var asset = TMP_FontAsset.CreateFontAsset(font, SAMPLING_SIZE, PADDING, GlyphRenderMode.SDFAA, ATLAS_SIZE, ATLAS_SIZE,
                AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: false);
            asset.name = $"{fontName} SDF";
            if (!asset.TryAddCharacters(Characters, out var missing) && !string.IsNullOrEmpty(missing))
                Debug.Log($"[Fuentes] {fontName}: sin glifo para «{missing}» (se usa la fuente de respaldo).");

            // Tabla fija: el juego no la modifica al jugar (sin cambios en el repositorio ni trabajo en tiempo real).
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorAssets.EnsureFolder(FONTS);
            AssetDatabase.CreateAsset(asset, path);
            asset.atlasTexture.name = $"{fontName} Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
            asset.material.name = $"{fontName} Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
