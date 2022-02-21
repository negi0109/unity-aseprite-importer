using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class Frame
    {
        public enum ChunkType
        {
            OldPalatte1 = 0x0004,
            OldPalatte2 = 0x0011,
            Layer = 0x2004,
            Cel = 0x2005,
            Tags = 0x2018,
            Palette = 0x2019,
        }

        public int magicNumber;
        public float duration;
        public int chunkCount;
        public List<AsepriteCel> cels = new List<AsepriteCel>();

        public static Frame Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var frame = new Frame();

            var frameEnd = reader.Position + reader.Dword();
            frame.magicNumber = reader.Word();
            if (frame.magicNumber != 0xF1FA) Aseprite.AsepriteFormatError();

            frame.chunkCount = reader.Word();
            frame.duration = reader.Word() / 1000f;
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
                    case ChunkType.Layer:
                        var layer = Layer.Deserialize(reader);
                        aseprite.layers.Add(layer);
                        break;
                    case ChunkType.Cel:
                        var cel = AsepriteCel.Deserialize(reader, aseprite);
                        frame.cels.Add(cel);

                        break;
                    case ChunkType.Tags:
                        var count = reader.Word();
                        reader.Seek(8);
                        for (int j = 0; j < count; j++)
                        {
                            aseprite.tags.Add(Tag.Deserialize(reader, aseprite));
                        }
                        break;
                    case ChunkType.Palette:
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

        public void GenerateTexture(Aseprite aseprite, Texture2D tex, Vector2Int start)
        {
            foreach (var cel in cels)
            {
                for (int x = 0; x < cel.size.x; x++)
                {
                    for (int y = 0; y < cel.size.y; y++)
                    {
                        var celPos = new Vector2Int(x, y);
                        var pos = cel.position + celPos;
                        var layer = aseprite.layers[cel.layer];
                        if (layer.flags.HasFlag(Layer.Flag.Visible) == false) continue;

                        if (pos.x >= 0 && pos.x < aseprite.header.size.x
                            && pos.y >= 0 && pos.y < aseprite.header.size.y)
                        {
                            var pixel = cel.pixels[celPos.x, celPos.y];
                            var color = Layer.blendFuncs[layer.blendMode](
                                pixel.GetColor(aseprite),
                                tex.GetPixel(start.x + pos.x, start.y + aseprite.header.size.y - 1 - pos.y),
                                cel.opacity * layer.opacity
                            );

                            tex.SetPixel(start.x + pos.x, start.y + aseprite.header.size.y - 1 - pos.y, color);
                        }
                    }
                }
            }
        }

        public Texture2D GenerateTexture(Aseprite aseprite)
        {
            var tex = new Texture2D(aseprite.header.size.x, aseprite.header.size.y);
            for (int x = 0; x < tex.width; x++)
                for (int y = 0; y < tex.height; y++)
                    tex.SetPixel(x, y, Color.clear);

            GenerateTexture(aseprite, tex, Vector2Int.zero);
            tex.Apply();

            return tex;
        }
    }
}
