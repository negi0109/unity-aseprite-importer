using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteCel
    {
        public int layer;
        public Vector2Int position;
        public float opacity;
        public int type;
        public Vector2Int size;
        public AsepritePixel[,] pixels;

        public static AsepriteCel Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var cel = new AsepriteCel();
            cel.layer = reader.Word();
            cel.position.x = reader.Short();
            cel.position.y = reader.Short();
            cel.opacity = reader.Byte() / 255f;
            cel.type = reader.Word();
            reader.Seek(7);

            if (cel.type == 0)
            {
                cel.size.x = reader.Word();
                cel.size.y = reader.Word();
                cel.pixels = cel.ToPixels(reader, cel.size, aseprite);
            }
            else if (cel.type == 1)
            {
                var frame = reader.Word();
                var linked = aseprite.frames[frame].cels.Find(other => other.layer == cel.layer);
                return linked;
            }
            else if (cel.type == 2)
            {
                cel.size.x = reader.Word();
                cel.size.y = reader.Word();
                reader.Seek(2);

                var stream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                cel.pixels = cel.ToPixels(new AsepriteReader(stream), cel.size, aseprite);
            }

            return cel;
        }

        public AsepritePixel[,] ToPixels(AsepriteReader reader, Vector2Int size, Aseprite aseprite)
        {
            var pixels = new AsepritePixel[size.x, size.y];

            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    pixels[x, y] = AsepritePixel.Deserialize(reader, aseprite);

            return pixels;
        }
    }
}
