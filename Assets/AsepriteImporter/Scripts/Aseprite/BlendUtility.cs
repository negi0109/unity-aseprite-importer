using System;
using UnityEngine;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public static class BlendUtility
    {
        public static Color Normal(Color fg, Color bg, float opacity)
        {
            fg.a *= opacity;

            var o = Color.clear;
            o.a = fg.a + bg.a * (1 - fg.a);
            o.r = (fg.r * fg.a + bg.r * bg.a * (1 - fg.a)) / o.a;
            o.g = (fg.g * fg.a + bg.g * bg.a * (1 - fg.a)) / o.a;
            o.b = (fg.b * fg.a + bg.b * bg.a * (1 - fg.a)) / o.a;
            if (o.a == 0) o = Color.clear;

            return o;
        }

        public static void ColorToHsl(Color color, out float h, out float s, out float l)
        {
            var max = Mathf.Max(color.r, color.g, color.b);
            var min = Mathf.Min(color.r, color.g, color.b);

            if (max == min) h = 0;
            else if (max == color.r) h = (color.g - color.b) / (max - min) * 60;
            else if (max == color.g) h = (color.b - color.r) / (max - min) * 60 + 120;
            else h = (color.r - color.g) / (max - min) * 60 + 240;

            if (h < 0) h += 360;

            l = 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;
            s = max - min;
        }

        public static Color Sat(Color color, float sat)
        {
            (int i, float v) min, mid, max;
            Compare(
                color.r, color.g, color.b,
                out min, out mid, out max
            );

            if (max.v > min.v)
            {
                color[mid.i] = (mid.v - min.v) * sat / (max.v - min.v);
                color[max.i] = sat;
            }
            else
            {
                color[mid.i] = color[max.i] = 0;
            }

            color[min.i] = 0;

            return color;
        }

        public static Color Lum(Color color, float lum)
        {
            float l;
            ColorToHsl(color, out _, out _, out l);

            float d = lum - l;
            color.r += d;
            color.g += d;
            color.b += d;

            ColorToHsl(color, out _, out _, out l);
            float n = Mathf.Min(color[0], color[1], color[2]);
            float x = Mathf.Max(color[0], color[1], color[2]);

            if (n < 0)
            {
                color.r = l + (color.r - l) * l / (l - n);
                color.g = l + (color.g - l) * l / (l - n);
                color.b = l + (color.b - l) * l / (l - n);
            }
            if (x > 1)
            {
                color.r = l + (color.r - l) * (1f - l) / (x - l);
                color.g = l + (color.g - l) * (1f - l) / (x - l);
                color.b = l + (color.b - l) * (1f - l) / (x - l);
            }

            return color;
        }

        public static void Compare(
            float a0, float a1, float a2,
            out (int i, float v) min, out (int i, float v) mid, out (int i, float v) max
        )
        {
            var a = new (int i, float v)[] { (0, a0), (1, a1), (2, a2) };
            Array.Sort(a, (
                (int i, float v) v1, (int i, float v) v2
            ) => v1.v.CompareTo(v2.v));

            min = a[0];
            mid = a[1];
            max = a[2];
        }

        public static Color SetAlpha(Color color)
        {
            color.r *= color.a;
            color.g *= color.a;
            color.b *= color.a;

            return color;
        }

        public static Color Blend(Color fg, Color bg, Func<float, float, float> rgbFunc, float a)
            => new Color(rgbFunc(fg.r, bg.r), rgbFunc(fg.g, bg.g), rgbFunc(fg.b, bg.b), a);
    }
}
