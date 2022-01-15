using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteFrame
    {
        public enum ChunkType
        {
            OldPalatte1 = 0x0004,
            OldPalatte2 = 0x0011,
            Layer = 0x2004,
            Cel = 0x2005,
            Palette = 0x2019,
        }

        public int magicNumber;
        public int duration;
        public int chunkCount;
        public List<Cel> cels = new List<Cel>();

        public static AsepriteFrame Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var frame = new AsepriteFrame();

            var frameEnd = reader.Position + reader.Dword();
            frame.magicNumber = reader.Word();
            if (frame.magicNumber != 0xF1FA) Aseprite.AsepriteFormatError();

            frame.chunkCount = reader.Word();
            frame.duration = reader.Word();
            reader.Seek(2);
            reader.Seek(4);

            for (int i = 0; i < frame.chunkCount; i++)
            {
                var chunkEnd = reader.Position + reader.Dword();
                ChunkType chunkType = (ChunkType)reader.Word();

                switch (chunkType)
                {
                    case ChunkType.OldPalatte1:
                    case ChunkType.OldPalatte2:
                        // 現在未使用
                        break;
                    case ChunkType.Cel:
                        var cel = Cel.Deserialize(reader, aseprite);
                        frame.cels.Add(cel);

                        break;
                    case ChunkType.Palette:
                        Debug.Log("Palette");
                        var size = reader.Dword();
                        var first = (int)reader.Dword();
                        var last = (int)reader.Dword();
                        reader.Seek(8);

                        for (int paletteIndex = first; paletteIndex <= last; paletteIndex++)
                        {
                            var flags = reader.Word();
                            var r = reader.Byte() / 255f;
                            var g = reader.Byte() / 255f;
                            var b = reader.Byte() / 255f;
                            var a = reader.Byte() / 255f;

                            // hasName
                            if (flags == 1) reader.String();
                            var color = new Color(r, g, b, a);
                            aseprite.palatte[paletteIndex] = color;
                        }
                        break;
                    default:
                        break;
                }

                reader.Position = chunkEnd;
            }

            frame.cels.Sort((a, b) => a.layer.CompareTo(b.layer));

            reader.Position = frameEnd;
            return frame;
        }

        public Texture2D GenerateTexture(Aseprite aseprite)
        {
            var tex = new Texture2D(aseprite.header.size.x, aseprite.header.size.y);

            foreach (var cel in cels)
            {
                for (int x = 0; x < cel.size.x; x++)
                {
                    for (int y = 0; y < cel.size.y; y++)
                    {
                        var celPos = new Vector2Int(x, y);
                        var pos = cel.position + celPos;

                        if (pos.x >= 0 && pos.x < aseprite.header.size.x
                            && pos.y >= 0 && pos.y < aseprite.header.size.y)
                        {
                            var pixel = cel.pixels[celPos.x, celPos.y];
                            tex.SetPixel(pos.x, aseprite.header.size.y - 1 - pos.y, pixel.GetColor(aseprite));
                        }
                    }
                }
            }
            tex.Apply();

            return tex;
        }

        public class Cel
        {
            public int layer;
            public Vector2Int position;
            public int opacity;
            public int type;
            public Vector2Int size;
            public AsepritePixel[,] pixels;

            public static Cel Deserialize(AsepriteReader reader, Aseprite aseprite)
            {
                var cel = new Cel();
                cel.layer = reader.Word();
                cel.position.x = reader.Short();
                cel.position.y = reader.Short();
                cel.opacity = reader.Byte();
                cel.type = reader.Word();
                reader.Seek(7);

                if (cel.type == 0)
                {
                    cel.size.x = reader.Word();
                    cel.size.y = reader.Word();
                    cel.pixels = cel.ToPixels(reader, cel.size, aseprite);
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
}
