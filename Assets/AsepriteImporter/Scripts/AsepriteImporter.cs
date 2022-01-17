using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Negi0109.AsepriteImporter
{
    [ScriptedImporter(0, "aseprite")]
    public class AsepriteImporter : ScriptedImporter
    {
        [Serializable]
        public class Separate
        {
            public string name;
            public bool visible = true;

            [CustomPropertyDrawer(typeof(Separate))]
            public class Drawer : PropertyDrawer
            {
                public static float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    var rect = position;
                    rect.height = LineHeight;
                    {
                        rect.width -= 20;
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("name"), label);
                        rect.x = rect.x + rect.width + 5;
                        rect.width = 20;
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("visible"), GUIContent.none);
                    }
                }

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return LineHeight;
                }
            }
        }
        // public Aseprite aseprite;

        public bool separateX;

        public Separate[] separates;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var tmp = Aseprite.Deserialize(bytes);

            // aseprite = tmp;
        }
    }
}
