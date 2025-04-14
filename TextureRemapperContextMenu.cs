using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.NnUtils.Modules.TextureRemapper
{
    public static class TextureRemapperContextMenu
    {
        [MenuItem("Assets/Texture Remapper", true)]
        private static bool ValidateTextureRemapper() =>
            Selection.objects.Length > 0 && AllSelectedAreTextures();

        [MenuItem("Assets/Texture Remapper")]
        private static void OpenTextureRemapperWithSelection()
        {
            var window = EditorWindow.GetWindow<TextureRemapper>("Texture Remapper");
            var textures = new List<Texture2D>();

            foreach (var obj in Selection.objects)
                if (obj is Texture2D tex) textures.Add(tex);

            window.InitializeWithTextures(textures);
        }

        private static bool AllSelectedAreTextures() =>
            Selection.objects.All(obj => obj is Texture2D);
    }
}