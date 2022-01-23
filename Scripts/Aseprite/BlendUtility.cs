using UnityEngine;

namespace Negi0109.AsepriteImporter
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
    }
}
