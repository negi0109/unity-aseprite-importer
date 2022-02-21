using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class Pixel
    {
        public Color color;
        public int paletteIndex = -1;
        public bool set = false;

        public static Pixel Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var pixel = new Pixel();
            if (aseprite.header.colorDepth == Header.ColorDepth.RGBA)
            {
                var r = reader.Byte() / 255f;
                var g = reader.Byte() / 255f;
                var b = reader.Byte() / 255f;
                var a = reader.Byte() / 255f;

                pixel.color = new Color(r, g, b, a);
                pixel.set = true;
            }
            else if (aseprite.header.colorDepth == Header.ColorDepth.Grayscale)
            {
                var v = reader.Byte() / 255f;
                var a = reader.Byte() / 255f;
                pixel.color = new Color(v, v, v, a);
                pixel.set = true;
            }
            else
            {
                pixel.paletteIndex = reader.Byte();
            }


            return pixel;
        }

        public Color GetColor(Aseprite aseprite)
        {
            if (!set)
            {
                color = aseprite.palatte[paletteIndex];
                set = true;
            }

            return color;
        }
    }
}
