#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace NnUtils.Modules.TextureRemapper.Editor
{
    public class TextureRemapperWindow : EditorWindow
    {
        private List<TextureMapping> _textureMappings = new();
        private string _outputName = "RemappedTexture";
        private Vector2 _scrollPosition;

        [MenuItem("NnUtils/Texture Remapper")]
        [Shortcut("NnUtils/Texture Remapper", KeyCode.R, ShortcutModifiers.Alt)]
        public static void ShowWindow()
        {
            if (ValidateContextOption()) OpenWithSelection();
            else GetWindow<TextureRemapperWindow>("Texture Remapper");
        }

        [MenuItem("Assets/NnUtils/Texture Remapper", true)]
        private static bool ValidateContextOption() => Selection.objects.All(o => o is Texture2D);

        [MenuItem("Assets/NnUtils/Texture Remapper", priority = -10)]
        private static void OpenWithSelection()
        {
            var window = GetWindow<TextureRemapperWindow>("Texture Remapper");
            window._textureMappings = Selection.objects
                .OfType<Texture2D>()
                .Select(t => new TextureMapping(t))
                .ToList();
        }

        private void OnEnable()
        {
            if (_textureMappings.Count == 0) _textureMappings.Add(new());
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Texture Remapper", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _textureMappings.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(_textureMappings[i].Texture
                    ? _textureMappings[i].Texture.name
                    : $"Texture {i + 1}");
                _textureMappings[i].Texture = (Texture2D)EditorGUILayout.ObjectField("",
                    _textureMappings[i].Texture, typeof(Texture2D), false, GUILayout.Width(65));

                EditorGUILayout.LabelField("Channel Mappings", EditorStyles.boldLabel);
                var mappings = _textureMappings[i].ChannelMappings;

                for (int j = 0; j < mappings.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    string[] channels = { "R", "G", "B", "A" };

                    EditorGUILayout.LabelField("From", GUILayout.Width(35));
                    mappings[j].InputChannel = EditorGUILayout.Popup(mappings[j].InputChannel,
                        channels, GUILayout.Width(30));

                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("To", GUILayout.Width(20));
                    mappings[j].OutputChannel = EditorGUILayout.Popup(mappings[j].OutputChannel,
                        channels, GUILayout.Width(30));

                    GUILayout.Space(10);
                    mappings[j].Invert =
                        GUILayout.Toggle(mappings[j].Invert, "", GUILayout.Width(18));
                    EditorGUILayout.LabelField("Invert", GUILayout.Width(50));

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove Mapping", GUILayout.Width(120)))
                    {
                        mappings.RemoveAt(j);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Mapping", GUILayout.Width(120)))
                {
                    mappings.Add(new());
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    break;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Texture", GUILayout.Width(120)))
                {
                    _textureMappings.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(5);
            if (GUILayout.Button("Add Texture", GUILayout.Width(120)))
                _textureMappings.Add(new());

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = ValidateInputs();
            if (GUILayout.Button("Create Texture", GUILayout.Width(150), GUILayout.Height(30)))
                TextureRemapper.RemapTextures(_textureMappings);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            GUI.enabled = true;
        }

        private bool ValidateInputs() =>
            !string.IsNullOrEmpty(_outputName) &&
            _textureMappings.All(m => m.Texture && m.ChannelMappings.Count != 0);
    }
}

#endif