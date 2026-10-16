using System.IO;
using UnityEditor;
using UnityEngine;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Sprites dibujados por código en edición (formas de interfaz, viñetas): se escriben una vez como PNG y se importan
    /// como sprite sin mipmaps. Si el archivo ya existe se reutiliza; para redibujarlo, se borra.
    /// </summary>
    public static class GeneratedSprites
    {
        public static Sprite GetOrCreate(string path, int width, int height, Color[] pixels, int border = 0)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            EditorAssets.EnsureFolder(Path.GetDirectoryName(path));
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
