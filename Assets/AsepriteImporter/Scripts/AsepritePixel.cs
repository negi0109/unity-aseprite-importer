using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepritePixel
    {
        public Color color;
        public int colorIndex;

        public static AsepritePixel Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var pixel = new AsepritePixel();
            if (aseprite.header.colorDepth == AsepriteHeader.ColorDepth.RGBA)
            {
                var r = reader.Byte() / 255f;
                var g = reader.Byte() / 255f;
                var b = reader.Byte() / 255f;
                var a = reader.Byte() / 255f;

                pixel.color = new Color(r, g, b, a);
            }
            else if (aseprite.header.colorDepth == AsepriteHeader.ColorDepth.Grayscale)
            {
                var v = reader.Byte() / 255f;
                var a = reader.Byte() / 255f;
                pixel.color = new Color(v, v, v, a);
            }
            else
            {
                pixel.colorIndex = reader.Byte();
            }


            return pixel;
        }
    }
}
