using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.NnUtils.Modules.TextureRemapper
{
    public static class TextureRemapper
    {
        private class TextureSettings
        {
            public bool IsReadable;
            public TextureImporterCompression Compression;
            public bool SRGB;
            public string Path;
        }

        public static void RemapTextures(List<TextureMapping> textureMappings, string outputName)
        {
            if (textureMappings.Count == 0 || !textureMappings[0].Texture)
            {
                EditorUtility.DisplayDialog("Error", "No textures selected!", "OK");
                return;
            }

            Dictionary<Texture2D, TextureSettings> originalSettings = new();
            foreach (var mapping in textureMappings.Where(m => m.Texture))
                originalSettings[mapping.Texture] = MakeTextureReadable(mapping.Texture);

            int width = 0, height = 0;
            foreach (var mapping in textureMappings.Where(m => m.Texture))
            {
                width = Mathf.Max(width, mapping.Texture.width);
                height = Mathf.Max(height, mapping.Texture.height);
            }

            Texture2D outputTexture = new(width, height, TextureFormat.RGBA32, false);
            Color defaultColor = new(0, 0, 0, 1);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    outputTexture.SetPixel(x, y, defaultColor);

            foreach (var mapping in textureMappings)
            {
                if (!mapping.Texture) continue;
                foreach (var chMap in mapping.ChannelMappings)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var u = x / (float)(width - 1);
                            var v = y / (float)(height - 1);
                            var srcX = Mathf.FloorToInt(u * (mapping.Texture.width - 1));
                            var srcY = Mathf.FloorToInt(v * (mapping.Texture.height - 1));

                            var srcColor = mapping.Texture.GetPixel(srcX, srcY);
                            var outColor = outputTexture.GetPixel(x, y);

                            var value = chMap.InputChannel switch
                            {
                                0 => srcColor.r,
                                1 => srcColor.g,
                                2 => srcColor.b,
                                3 => srcColor.a,
                                _ => 0
                            };

                            if (chMap.Invert) value = 1.0f - value;

                            switch (chMap.OutputChannel)
                            {
                                case 0: outColor.r = value; break;
                                case 1: outColor.g = value; break;
                                case 2: outColor.b = value; break;
                                case 3: outColor.a = value; break;
                            }

                            outputTexture.SetPixel(x, y, outColor);
                        }
                    }
                }
            }

            outputTexture.Apply();

            var path = AssetDatabase.GetAssetPath(textureMappings[0].Texture);
            var directory = Path.GetDirectoryName(path);
            var newPath = Path.Combine(directory, $"{outputName}.png");
            if (File.Exists(newPath))
            {
                var replace = EditorUtility.DisplayDialog(
                    "File Exists",
                    $"A file named \"{outputName}\" already exists. Do you want to replace it?",
                    "Replace",
                    "Cancel"
                );
                if (!replace) return;
            }

            File.WriteAllBytes(newPath, outputTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            foreach (var kvp in originalSettings)
                RestoreTextureSettings(kvp.Value);

            var newImporter = AssetImporter.GetAtPath(newPath) as TextureImporter;
            if (newImporter)
            {
                newImporter.isReadable = false;
                newImporter.sRGBTexture = false;
                AssetDatabase.ImportAsset(newPath);
            }

            Object.DestroyImmediate(outputTexture);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
        }

        private static TextureSettings MakeTextureReadable(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            var settings = new TextureSettings
            {
                IsReadable = importer.isReadable,
                Compression = importer.textureCompression,
                SRGB = importer.sRGBTexture,
                Path = path
            };

            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = false;
            AssetDatabase.ImportAsset(path);

            return settings;
        }

        private static void RestoreTextureSettings(TextureSettings settings)
        {
            var importer = AssetImporter.GetAtPath(settings.Path) as TextureImporter;
            if (!importer) return;

            importer.isReadable = settings.IsReadable;
            importer.textureCompression = settings.Compression;
            importer.sRGBTexture = settings.SRGB;
            AssetDatabase.ImportAsset(settings.Path);
        }
    }
}
