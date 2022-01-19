using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Negi0109.AsepriteImporter
{
    [CustomEditor(typeof(AsepriteImporter))]
    public class AsepriteImporterEditor : ScriptedImporterEditor
    {
        private Aseprite aseprite;
        private Texture2D texture;

        private bool previewToggle = true;
        private float previewScale = 10;
        private const int PREVIEW_WIDTH = 300;

        private bool layersToggle = false;

        public void Reload()
        {
            var importer = target as AssetImporter;
            var bytes = File.ReadAllBytes(importer.assetPath);
            aseprite = Aseprite.Deserialize(bytes);
            texture = aseprite.GenerateTexture();
            texture.filterMode = FilterMode.Point;
            var width = (float)PREVIEW_WIDTH / aseprite.header.size.x;
            previewScale = Mathf.Min(previewScale, width);
        }

        public override void OnEnable()
        {
            if (aseprite == null) Reload();
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            if (aseprite == null) Reload();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("pixelsPerUnit"));
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
            }

            ApplyRevertGUI();
        }
    }
}
