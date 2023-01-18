using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    [CustomEditor(typeof(AsepriteImporter))]
    public class AsepriteImporterEditor : ScriptedImporterEditor
    {
        private Aseprite.Aseprite aseprite;
        private Texture2D texture;

        #region PREVIEW

        private bool previewToggle = false;
        private float previewScale = 10;
        private const int PREVIEW_WIDTH = 300;

        private bool layersToggle = false;

        #endregion

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
            if (aseprite == null) LoadAseprite();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("pixelsPerUnit"));

            var edging = serializedObject.FindProperty("edging");
            edging.boolValue = EditorGUILayout.Toggle("edging", edging.boolValue);

            var separateX = serializedObject.FindProperty("separateX");
            separateX.boolValue = EditorGUILayout.Toggle("SeparateX", separateX.boolValue);
            if (separateX.boolValue)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("separatesX"),
                    new GUIContent("separatesX")
                );
            }

            var separateY = serializedObject.FindProperty("separateY");
            separateY.boolValue = EditorGUILayout.Toggle("separateY", separateY.boolValue);
            if (separateY.boolValue)
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("separatesY"),
                    new GUIContent("separatesY")
                );
            }

            var exportAnimation = serializedObject.FindProperty("exportAnimation");
            EditorGUILayout.PropertyField(exportAnimation);

            if (exportAnimation.boolValue)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);

                var separateTags = serializedObject.FindProperty("separateTags");
                separateTags.boolValue = EditorGUILayout.Toggle("Separate Tag", separateTags.boolValue);

                if (separateTags.boolValue)
                {
                    var tagSettings = serializedObject.FindProperty("tagSettings");
                    var tagSettingsSize = tagSettings.arraySize;
                    var dic = new Dictionary<string, SerializedProperty>();
                    {
                        var deleted = 0;

                        for (var i = 0; i < tagSettingsSize; i++)
                        {
                            var s = tagSettings.GetArrayElementAtIndex(i - deleted);
                            var name = s.FindPropertyRelative("name").stringValue;
                            var exists = aseprite.tags.Exists(t => t.name.Equals(name));
                            if (exists) dic.Add(name, s);
                            else { tagSettings.DeleteArrayElementAtIndex(i); deleted++; };
                        }
                    }

                    EditorGUILayout.BeginVertical();

                    var existTagNames = new HashSet<string>();
                    foreach (var tag in aseprite.tags)
                    {
                        if (existTagNames.Contains(tag.name)) continue;
                        existTagNames.Add(tag.name);

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
                        if (dic.ContainsKey(tag.name))
                        {
                            var setting = dic[tag.name];
                            EditorGUILayout.PropertyField(setting);
                        }
                        else
                        {
                            tagSettings.arraySize++;
                            var setting = tagSettings.GetArrayElementAtIndex(tagSettings.arraySize - 1);
                            setting.FindPropertyRelative("name").stringValue = tag.name;
                            EditorGUILayout.PropertyField(setting);
                        }
                    }
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    var baseSetting = serializedObject.FindProperty("baseSetting");
                    EditorGUILayout.PropertyField(baseSetting);
                }
            }

            EditorGUILayout.Separator();

            {
                EditorGUILayout.LabelField("SecondaryTexture", EditorStyles.boldLabel);

#if !UNITY_2022_2_OR_NEWER
                EditorGUILayout.HelpBox(@"Output of SecondaryTexture is valid only in Unity2022.2 or later.
This Unity version only outputs as Texture2D.", MessageType.Info);
#endif
                var layerSettings = serializedObject.FindProperty("layerSettings");
                var layerSettingsSize = layerSettings.arraySize;
                var dic = new Dictionary<string, SerializedProperty>();
                {
                    var deleted = 0;

                    for (var i = 0; i < layerSettingsSize; i++)
                    {
                        var s = layerSettings.GetArrayElementAtIndex(i - deleted);
                        var name = s.FindPropertyRelative("name").stringValue;
                        var exists = aseprite.layers.Exists(l => l.name.Equals(name));
                        if (exists) dic.Add(name, s);
                        else { layerSettings.DeleteArrayElementAtIndex(i); deleted++; };
                    }
                }

                EditorGUILayout.BeginVertical();

                foreach (var layer in aseprite.layers)
                {
                    if (layer.childLevel == 0)
                    {
                        EditorGUI.indentLevel++;
                        if (dic.ContainsKey(layer.name))
                        {
                            var setting = dic[layer.name];

                            EditorGUILayout.BeginHorizontal();
                            var secondaryTexture = setting.FindPropertyRelative("secondaryTexture");
                            EditorGUILayout.PropertyField(secondaryTexture, new GUIContent(layer.name));

                            EditorGUI.BeginDisabledGroup(!secondaryTexture.boolValue);
                            var secondaryTextureName = setting.FindPropertyRelative("secondaryTextureName");
                            EditorGUILayout.PropertyField(secondaryTextureName, new GUIContent(""));
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            layerSettings.arraySize++;
                            var setting = layerSettings.GetArrayElementAtIndex(layerSettings.arraySize - 1);
                            setting.FindPropertyRelative("name").stringValue = layer.name;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();

            previewToggle = EditorGUILayout.Foldout(previewToggle, "aseprite");
            if (previewToggle)
            {
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
