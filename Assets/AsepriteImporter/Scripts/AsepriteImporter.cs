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
            public bool invisible = true;

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
                        var invisible = property.FindPropertyRelative("invisible");
                        invisible.boolValue = !EditorGUI.Toggle(rect, !invisible.boolValue);
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
        public float pixelsPerUnit = 100f;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var aseprite = Aseprite.Deserialize(bytes);
            var texture = aseprite.GenerateTexture();
            texture.filterMode = FilterMode.Point;

            ctx.AddObjectToAsset("texture", texture);
            ctx.SetMainObject(texture);
            if (separateX && separates.Length > 0)
            {
                var spriteSize = new Vector2(aseprite.header.size.x / separates.Length, aseprite.header.size.y);

                for (int i = 0; i < separates.Length; i++)
                {
                    var separate = separates[i];
                    if (separate.invisible) continue;

                    for (int j = 0; j < aseprite.frames.Length; j++)
                    {
                        var frame = aseprite.frames[j];
                        var sprite = Sprite.Create(
                            texture,
                            new Rect(spriteSize.x * i, spriteSize.y * j, spriteSize.x, spriteSize.y),
                            new Vector2(.5f, .5f),
                            pixelsPerUnit
                        );
                        sprite.name = $"{separate.name}-{j}";
                        ctx.AddObjectToAsset($"{i}-{j}", sprite);
                    }
                }
            }
            else
            {
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f),
                    pixelsPerUnit
                );

                ctx.AddObjectToAsset("sprite", sprite);
            }
        }
    }
}
