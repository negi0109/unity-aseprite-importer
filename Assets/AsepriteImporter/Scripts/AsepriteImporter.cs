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
        public bool exportAnimation;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var separates = this.separates;
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var aseprite = Aseprite.Deserialize(bytes);
            var texture = aseprite.GenerateTexture();
            texture.filterMode = FilterMode.Point;

            ctx.AddObjectToAsset("texture", texture);
            ctx.SetMainObject(texture);
            if (!separateX || separates.Length == 0)
            {
                separates = new Separate[]{
                    new Separate(){ name = Path.GetFileNameWithoutExtension(ctx.assetPath), invisible = false }
                };
            }

            var spriteSize = new Vector2(aseprite.header.size.x / separates.Length, aseprite.header.size.y);

            for (int i = 0; i < separates.Length; i++)
            {
                var sprites = new Sprite[aseprite.header.frames];
                var separate = separates[i];
                if (separate.invisible) continue;

                for (int j = 0; j < aseprite.header.frames; j++)
                {
                    var frame = aseprite.frames[j];
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(spriteSize.x * i, spriteSize.y * j, spriteSize.x, spriteSize.y),
                        new Vector2(.5f, .5f),
                        pixelsPerUnit
                    );
                    sprite.name = $"{separate.name}-{j}";
                    sprites[j] = sprite;
                    ctx.AddObjectToAsset($"{i}-{j}", sprite);
                }

                if (exportAnimation)
                {
                    var clip = new AnimationClip();
                    var curveBinding = new EditorCurveBinding();
                    curveBinding.type = typeof(SpriteRenderer);
                    curveBinding.path = "";
                    curveBinding.propertyName = "m_Sprite";

                    var keyframes = new ObjectReferenceKeyframe[aseprite.header.frames + 1];

                    var time = 0f;
                    for (int j = 0; j < aseprite.header.frames; j++)
                    {
                        var frame = aseprite.frames[j];
                        keyframes[j].time = time;
                        keyframes[j].value = sprites[j];

                        time += frame.duration;
                    }
                    keyframes[aseprite.header.frames].time = time;
                    keyframes[aseprite.header.frames].value = sprites[aseprite.header.frames - 1];

                    AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
                    clip.name = $"{separate.name}";
                    ctx.AddObjectToAsset($"{i}", clip);
                }
            }
        }
    }
}
