using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

namespace Negi0109.AsepriteImporter
{
    [ScriptedImporter(0, new string[] { "aseprite", "ase" })]
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
        public bool separateTags;

        public Separate[] separates;
        public float pixelsPerUnit = 100f;
        public bool exportAnimation;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var aseprite = Aseprite.Deserialize(bytes);
            var texture = aseprite.GenerateTexture();
            var separates = this.separates;
            var tags = aseprite.tags.ToArray();

            texture.filterMode = FilterMode.Point;

            ctx.AddObjectToAsset("texture", texture);
            ctx.SetMainObject(texture);
            if (!separateX || separates == null || separates.Length == 0)
            {
                separates = new Separate[]{
                    new Separate(){ name = Path.GetFileNameWithoutExtension(ctx.assetPath), invisible = false }
                };
            }
            if (!separateTags || aseprite.tags == null || aseprite.tags.Count == 0)
                tags = new AsepriteTag[] { new AsepriteTag() { name = "", from = 0, to = aseprite.header.frames - 1 } };

            var spriteSize = new Vector2(aseprite.header.size.x / separates.Length, aseprite.header.size.y);

            for (int i = 0; i < separates.Length; i++)
            {
                for (int j = 0; j < tags.Length; j++)
                {
                    var separate = separates[i];
                    var tag = tags[j];
                    var frames = tag.to - tag.from + 1;
                    var sprites = new Sprite[frames];

                    if (separate.invisible) continue;

                    for (int k = 0; k < frames; k++)
                    {
                        var frame = aseprite.frames[tag.from + k];
                        var sprite = Sprite.Create(
                            texture,
                            new Rect(spriteSize.x * i, spriteSize.y * (tag.from + k), spriteSize.x, spriteSize.y),
                            new Vector2(.5f, .5f),
                            pixelsPerUnit
                        );
                        sprite.name = $"{separate.name}-{tag.name}-{k}";
                        sprites[k] = sprite;
                        ctx.AddObjectToAsset($"{i}-{j}-{k}", sprite);
                    }

                    if (exportAnimation)
                    {
                        var clip = new AnimationClip();
                        var curveBinding = new EditorCurveBinding();
                        curveBinding.type = typeof(SpriteRenderer);
                        curveBinding.path = "";
                        curveBinding.propertyName = "m_Sprite";

                        var keyframes = new ObjectReferenceKeyframe[frames + 1];

                        var time = 0f;
                        for (int k = 0; k < frames; k++)
                        {
                            var frame = aseprite.frames[tag.from + k];
                            keyframes[k].time = time;
                            keyframes[k].value = sprites[k];

                            time += frame.duration;
                        }
                        keyframes[frames].time = time;
                        keyframes[frames].value = sprites[frames - 1];

                        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
                        clip.name = $"{separate.name}-{tag.name}";
                        ctx.AddObjectToAsset($"{i}-{j}", clip);
                    }
                }
            }
        }
    }
}
