using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Negi0109.AsepriteImporter
{
    [CustomEditor(typeof(AsepriteImporter))]
    public class AsepriteImporterEditor : ScriptedImporterEditor
    {
        private Aseprite.Aseprite aseprite;
        private Texture2D texture;

        private bool previewToggle = false;
        private float previewScale = 10;
        private const int PREVIEW_WIDTH = 300;

        private bool layersToggle = false;
        private bool tagsToggle = false;

        public void LoadAseprite()
        {
            var importer = target as AssetImporter;
            var bytes = File.ReadAllBytes(importer.assetPath);
            aseprite = Aseprite.Aseprite.Deserialize(bytes);
            texture = aseprite.GenerateTexture();
            texture.filterMode = FilterMode.Point;
            var width = (float)PREVIEW_WIDTH / aseprite.header.size.x;
            previewScale = Mathf.Min(previewScale, width);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pixelsPerUnit"));
            var separateTags = serializedObject.FindProperty("separateTags");
            separateTags.boolValue = EditorGUILayout.Toggle("Tag", separateTags.boolValue);

            var separateX = serializedObject.FindProperty("separateX");
            separateX.boolValue = EditorGUILayout.Toggle("Separate", separateX.boolValue);

            if (separateX.boolValue)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("separates"),
                    new GUIContent("separates")
                );
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("exportAnimation"));

            previewToggle = EditorGUILayout.Foldout(previewToggle, "aseprite");
            if (previewToggle)
            {
                if (aseprite == null) LoadAseprite();

                layersToggle = EditorGUILayout.Foldout(layersToggle, "Layers");
                if (layersToggle)
                {
                    EditorGUILayout.BeginVertical();
                    foreach (var layer in aseprite.layers)
                    {
                        EditorGUI.indentLevel = 1 + layer.childLevel;
                        EditorGUILayout.LabelField(layer.name);
                    }
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.EndVertical();
                }

                tagsToggle = EditorGUILayout.Foldout(tagsToggle, "Tags");
                if (tagsToggle)
                {
                    EditorGUILayout.BeginVertical();
                    foreach (var tag in aseprite.tags)
                    {
                        var rect = EditorGUILayout.GetControlRect(false);
                        var color = new Rect(rect);
                        var label = new Rect(rect);
                        color.width = rect.height;
                        label.x += rect.height + 5f;
                        label.width = label.width - rect.height - 5f;

                        var colorStyle = new GUIStyle(GUI.skin.box);
                        var colorTex = new Texture2D(1, 1);
                        colorTex.SetPixels(new Color[] { tag.color });
                        colorTex.Apply();
                        colorStyle.normal.background = colorTex;

                        GUI.Box(color, "", colorStyle);
                        EditorGUI.LabelField(label, $"{tag.from,2} - {tag.to,2} : {tag.name}");
                    }
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.EndVertical();
                }

                previewScale = EditorGUILayout.Slider("preview scale", previewScale, 1, 100);

                var size = aseprite.header.size;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Frames");
                for (int i = 0; i < aseprite.frames.Length; i++)
                {
                    EditorGUILayout.LabelField($"{i}:");
                    var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, size.y * previewScale));
                    var height = 1f / aseprite.frames.Length;
                    GUI.DrawTextureWithTexCoords(
                        new Rect(rect.x, rect.y, size.x * previewScale, size.y * previewScale),
                        texture,
                        new Rect(0, height * i, 1, height)
                    );
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
            }

            ApplyRevertGUI();
        }
    }
}
