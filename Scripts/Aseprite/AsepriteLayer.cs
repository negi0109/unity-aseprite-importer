using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class Layer
    {
        [Flags]
        public enum Flag
        {
            Visible = 1,
            Editable = 2,
            Lock = 4,
            Background = 8,
            LinkedCels = 16,
            LayerGroupShouldDisplayedCollapsed = 32,
            Reference = 64,
        }
        public enum Type
        {
            Normal = 0,
            Group = 1,
            Tilemap = 2,
        }

        public Flag flags;
        public Type type;
        public int childLevel;
        public Vector2Int size;
        public int blendMode;
        public static Func<Color, Color, float, Color>[] blendFuncs = new Func<Color, Color, float, Color>[]
        {
            // Normal
            BlendUtility.Normal,
            // Multiply
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => f * b, fg.a),
                bg,
                opacity
            ),
            // Screen
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => 1 - (1 - f) * (1 - b), fg.a),
                bg,
                opacity
            ),
            // Overlay
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => b > 0.5f ?
                        (b * 2 - 1) + f - (b * 2 - 1) * f
                        : b * 2 * f,
                    fg.a
                ),
                bg,
                opacity
            ),
            // Darken
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => Mathf.Min(f, b), fg.a),
                bg,
                opacity
            ),
            // Lighten
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => Mathf.Max(f, b), fg.a),
                bg,
                opacity
            ),
            // ColorDodge
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => b == 0 ? 0 : b >= 1 - f ? 1 : b / (1 - f),
                    fg.a
                ),
                bg,
                opacity
            ),
            // ColorBurn
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => b == 1 ? 1 : 1 - b >= f ? 0 : 1 - (1 - b) / f,
                    fg.a
                ),
                bg,
                opacity
            ),
            // HardLight
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => f > 0.5f ?
                        (f * 2 - 1) + b - (f * 2 - 1) * b
                        : f * 2 * b,
                    fg.a
                ),
                bg,
                opacity
            ),
            // SoftLight
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => ((1 - b) * f + (1 - (1 - f) * (1 - b))) * b,
                    fg.a
                ),
                bg,
                opacity
            ),
            // Difference
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => Mathf.Abs(f - b),
                    fg.a
                ),
                bg,
                opacity
            ),
            // Exclusion
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(
                    fg,
                    bg,
                    (f, b) => f + b - 2 * b * f,
                    fg.a
                ),
                bg,
                opacity
            ),
            // Hue
            (fg, bg, opacity) => {
                bg = BlendUtility.SetAlpha(bg);
                fg = BlendUtility.SetAlpha(fg);

                float s, l;
                BlendUtility.ColorToHsl(bg, out _, out s, out l);

                var color = BlendUtility.Lum(BlendUtility.Sat(fg, s), l);
                color.a = fg.a;

                return BlendUtility.Normal(
                    color,
                    bg,
                    opacity
                );
            },
            // Saturation
            (fg, bg, opacity) => {
                bg = BlendUtility.SetAlpha(bg);
                fg = BlendUtility.SetAlpha(fg);

                float s, l;
                BlendUtility.ColorToHsl(bg, out _, out _, out l);
                BlendUtility.ColorToHsl(fg, out _, out s, out _);

                var color = BlendUtility.Lum(BlendUtility.Sat(bg, s), l);
                color.a = fg.a;

                return BlendUtility.Normal(
                    color,
                    bg,
                    opacity
                );
            },
            // Color
            (fg, bg, opacity) => {
                bg = BlendUtility.SetAlpha(bg);
                fg = BlendUtility.SetAlpha(fg);

                float l;
                BlendUtility.ColorToHsl(bg, out _, out _, out l);

                var color = BlendUtility.Lum(fg, l);
                color.a = fg.a;

                return BlendUtility.Normal(
                    color,
                    bg,
                    opacity
                );
            },
            // Luminosity
            (fg, bg, opacity) => {
                bg = BlendUtility.SetAlpha(bg);
                fg = BlendUtility.SetAlpha(fg);

                float l;
                BlendUtility.ColorToHsl(fg, out _, out _, out l);

                var color = BlendUtility.Lum(bg, l);
                color.a = fg.a;

                return BlendUtility.Normal(
                    color,
                    bg,
                    opacity
                );
            },
            // Addition
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => f + b, fg.a),
                bg,
                opacity
            ),
            // Subtract
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => Mathf.Max(0f, b - f), fg.a),
                bg,
                opacity
            ),
            // Divide
            (fg, bg, opacity) => BlendUtility.Normal(
                BlendUtility.Blend(fg, bg, (f, b) => f == 0 ? 1 : Mathf.Min(1f, b / f), fg.a),
                bg,
                opacity
            ),
        };

        public float opacity;
        public string name;
        public uint tilesetIndex;

        public static Layer Deserialize(AsepriteReader reader)
        {
            var layer = new Layer();
            layer.flags = (Flag)reader.Word();
            layer.type = (Type)reader.Word();
            layer.childLevel = reader.Word();
            layer.size.x = reader.Word();
            layer.size.y = reader.Word();
            layer.blendMode = reader.Word();
            layer.opacity = reader.Byte() / 255f;
            reader.Seek(3);
            layer.name = reader.String();

            if (layer.type == Type.Tilemap)
                layer.tilesetIndex = reader.Dword();

            return layer;
        }
    }
}
