using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

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
        [Serializable]
        public class TagSetting
        {
            public string name;
            public bool loopTime;

            [CustomPropertyDrawer(typeof(TagSetting))]
            public class Drawer : PropertyDrawer
            {
                public static float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    var rect = position;
                    rect.height = LineHeight;
                    {
                        rect.x += 20;
                        rect.width -= 20;
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("loopTime"), new GUIContent("looptime"));
                    }
                }

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return LineHeight;
                }
            }
        }

        [Serializable]
        public class LayerSetting
        {
            public string name;
            public bool secondaryTexture;
            public string secondaryTextureName;
        }

        // public Aseprite aseprite;

        public bool separateX;
        public bool separateY;
        public bool separateTags;

        public Separate[] separatesX;
        public Separate[] separatesY;

        public TagSetting[] tagSettings;
        public LayerSetting[] layerSettings = new LayerSetting[0];
        public float pixelsPerUnit = 100f;
        public bool exportAnimation;
        public bool edging;

        public TagSetting baseSetting;

        public Texture2D EdgingTexture(Texture2D texture, Vector2Int spriteSize)
        {
            var sx = texture.width / spriteSize.x;
            var sy = texture.height / spriteSize.y;

            var tex = new Texture2D(sx * (spriteSize.x + 1) + 2, sy * (spriteSize.y + 1) + 2);
            tex.filterMode = FilterMode.Point;

            for (int x = 0; x < tex.width; x++)
                for (int y = 0; y < tex.height; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int dx = 0; dx < spriteSize.x; dx++)
                        for (int dy = 0; dy < spriteSize.y; dy++)
                            tex.SetPixel(
                                x * (spriteSize.x + 1) + dx + 1,
                                y * (spriteSize.y + 1) + dy + 1,
                                texture.GetPixel(
                                    x * spriteSize.x + dx,
                                    y * spriteSize.y + dy
                                )
                            );

            return tex;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var aseprite = Aseprite.Aseprite.Deserialize(bytes);
            Texture2D texture = aseprite.GenerateTexture();
            SecondarySpriteTexture[] secondaryTextures;
            var separatesX = this.separatesX;
            var separatesY = this.separatesY;
            var tagSettings = this.tagSettings;
            var tags = aseprite.tags.ToArray();
            var baseName = Path.GetFileNameWithoutExtension(ctx.assetPath);

            // SecondaryTexture
            {
                var layerSettingHash = layerSettings.ToDictionary(layerSetting => layerSetting.name);

                var dic = new Dictionary<string, HashSet<int>>();
                var mainTexSet = new HashSet<int>();
                var current = 0;
                for (int i = 0; i < aseprite.layers.Count; i++)
                {
                    if (aseprite.layers[i].childLevel == 0) current = i;
                    var layerName = aseprite.layers[current].name;

                    if (layerSettingHash.ContainsKey(layerName) && layerSettingHash[layerName].secondaryTexture)
                    {
                        var secondaryTextureName = layerSettingHash[layerName].secondaryTextureName;
                        if (!dic.ContainsKey(secondaryTextureName)) dic.Add(secondaryTextureName, new HashSet<int>());
                        dic[secondaryTextureName].Add(i);
                    }
                    else
                    {
                        mainTexSet.Add(i);
                    }
                }

                texture = aseprite.GenerateTexture(mainTexSet);

                secondaryTextures = dic.Keys.Select(
                    key =>
                    {
                        var tex = new SecondarySpriteTexture() { name = key, texture = aseprite.GenerateTexture(dic[key]) };
                        tex.texture.name = key;
                        return tex;
                    }
                ).ToArray();
            }

            if (!separateX || separatesX == null || separatesX.Length == 0)
            {
                separatesX = new Separate[]{
                    new Separate(){ name = "0", invisible = false }
                };
            }
            if (!separateY || separatesY == null || separatesY.Length == 0)
            {
                separatesY = new Separate[]{
                    new Separate(){ name = "0", invisible = false }
                };
            }

            if (!separateTags || aseprite.tags == null || aseprite.tags.Count == 0)
            {
                tags = new Aseprite.Tag[] { new Aseprite.Tag() { name = "", from = 0, to = aseprite.header.frames - 1 } };
                tagSettings = new TagSetting[] { baseSetting ?? new TagSetting() };
                tagSettings[0].name = "";
            }

            {
                var dic = new Dictionary<string, TagSetting>();
                var tagCounts = new Dictionary<string, int>();
                var tmpTagSettings = new TagSetting[tags.Length];

                foreach (var tagSetting in tagSettings) {
                    if (!dic.ContainsKey(tagSetting.name)) dic[tagSetting.name] = tagSetting;
                }

                int index = 0;
                foreach (var tag in tags) {
                    var tagName = tag.name;
                    if (!tagCounts.ContainsKey(tagName)) tagCounts[tagName] = 0;
                    if (tagCounts[tagName] != 0) tag.name = $"{tagName}{tagCounts[tagName]}";

                    if (dic.TryGetValue(tagName, out TagSetting tagSetting)) {
                        tmpTagSettings[index] = tagSetting;
                    } else {
                        tmpTagSettings[index] = new TagSetting(){ name = tagName };
                    }

                    tagCounts[tagName]++;
                    index++;
                }

                tagSettings = tmpTagSettings;
            }

            var spriteSize = new Vector2Int(aseprite.header.size.x / separatesX.Length, aseprite.header.size.y / separatesY.Length);

            if (edging)
            {
                texture = EdgingTexture(texture, spriteSize);

                for (int i = 0; i < secondaryTextures.Length; i++)
                {
                    secondaryTextures[i].texture = EdgingTexture(secondaryTextures[i].texture, spriteSize);
                }
            }

            texture.filterMode = FilterMode.Point;

            ctx.AddObjectToAsset("texture", texture);
            for (int i = 0; i < secondaryTextures.Length; i++)
            {
                secondaryTextures[i].texture.filterMode = FilterMode.Point;
                ctx.AddObjectToAsset("texture-" + secondaryTextures[i].name, secondaryTextures[i].texture);
            }

            ctx.SetMainObject(texture);

            for (int x = 0; x < separatesX.Length; x++)
            {
                if (separatesX[x].invisible) continue;

                for (int y = 0; y < separatesY.Length; y++)
                {
                    if (separatesY[y].invisible) continue;

                    for (int j = 0; j < tags.Length; j++)
                    {
                        var tag = tags[j];
                        var tagSetting = tagSettings[j];
                        var frames = tag.to - tag.from + 1;
                        var sprites = new Sprite[frames];

                        for (int k = 0; k < frames; k++)
                        {
                            var frame = aseprite.frames[tag.from + k];

#if UNITY_2022_2_OR_NEWER
                            var sprite = Sprite.Create(
                                texture,
                                edging ?
                                    new Rect(
                                        (spriteSize.x + 1) * x + 1,
                                        (spriteSize.y + 1) * ((tag.from + k) * separatesY.Length + y) + 1,
                                        spriteSize.x, spriteSize.y) :
                                    new Rect(spriteSize.x * x, spriteSize.y * (tag.from + k), spriteSize.x, spriteSize.y),
                                new Vector2(.5f, .5f),
                                pixelsPerUnit, 0,
                                SpriteMeshType.Tight, Vector4.zero,
                                false, secondaryTextures
                            );
#else
                            var sprite = Sprite.Create(
                                texture,
                                edging ?
                                    new Rect(
                                        (spriteSize.x + 1) * x + 1,
                                        (spriteSize.y + 1) * ((tag.from + k) * separatesY.Length + y) + 1,
                                        spriteSize.x, spriteSize.y) :
                                    new Rect(spriteSize.x * x, spriteSize.y * (tag.from + k), spriteSize.x, spriteSize.y),
                                new Vector2(.5f, .5f),
                                pixelsPerUnit
                            );
#endif
                            sprite.name = baseName;
                            if (separateX) sprite.name += $"-{separatesX[x].name}";
                            if (separateY) sprite.name += $"-{separatesY[y].name}";
                            if (!String.IsNullOrEmpty(tag.name)) sprite.name += $"-{tag.name}";

                            sprites[k] = sprite;
                            ctx.AddObjectToAsset($"{x}-{y}-{tag.name}-{k}", sprite);
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
                            var animationSetting = new AnimationClipSettings();

                            {
                                animationSetting.loopTime = tagSetting.loopTime;
                            }

                            animationSetting.stopTime = time;
                            AnimationUtility.SetAnimationClipSettings(clip, animationSetting);

                            clip.name = baseName;
                            if (separateX) clip.name += $"-{separatesX[x].name}";
                            if (separateY) clip.name += $"-{separatesY[y].name}";
                            if (!String.IsNullOrEmpty(tag.name)) clip.name += $"-{tag.name}";

                            ctx.AddObjectToAsset($"{x}-{y}-{tag.name}-{j}", clip);
                        }
                    }
                }
            }
        }
    }
}
