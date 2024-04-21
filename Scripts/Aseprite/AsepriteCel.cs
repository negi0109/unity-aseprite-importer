using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class AsepriteCel
    {
        public int layer;
        public Vector2Int position;
        public float opacity;
        public int type;
        public int zIndex;
        public Vector2Int size;
        public Pixel[,] pixels;
        public int tileBitLength;
        public uint tileBitmask;
        public uint[,] tiles;

        public static AsepriteCel Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var cel = new AsepriteCel();
            cel.layer = reader.Word();
            cel.position.x = reader.Short();
            cel.position.y = reader.Short();
            cel.opacity = reader.Byte() / 255f;
            cel.type = reader.Word();
            cel.zIndex = reader.Short();
            reader.Seek(5);

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

                reader.Seek(2); // zlib stream unnecessary parts

                var stream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                cel.pixels = cel.ToPixels(new AsepriteReader(stream), cel.size, aseprite);
            }
            else if (cel.type == 3)
            {
                cel.size.x = reader.Word();
                cel.size.y = reader.Word();
                cel.tileBitLength = reader.Word(); // fixed 32-bit per tile

                cel.tileBitmask = reader.Dword();
                reader.Dword(); // unused, bitmask for x flip
                reader.Dword(); // unused, bitmask for y flip
                reader.Dword(); // unused, bitmask for d flip
                reader.Seek(10);

                reader.Seek(2); // zlib stream unnecessary parts
                var stream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                cel.tiles = cel.ToTiles(new AsepriteReader(stream), cel.size);
            }

            return cel;
        }

        public IEnumerable<(Vector2Int, Color)> GetColors(Aseprite aseprite, Layer layer)
        {
            if (type == 0 || type == 2)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        var pixel = pixels[x, y];
                        var color = pixel.GetColor(aseprite);

                        yield return (new Vector2Int(x, y), color);
                    }
                }
            }
            else if (type == 3)
            {
                var tileset = aseprite.GetTileset(layer.tilesetIndex);

                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        var tilePos = new Vector2Int(x, y);
                        foreach (var value in tileset.GetTileColors(aseprite, tiles[x, y]))
                        {
                            var (pos, color) = value;
                            yield return (
                                new Vector2Int(
                                    pos.x + tilePos.x * tileset.size.x,
                                    pos.y + tilePos.y * tileset.size.y
                                ),
                                color
                            );
                        }
                    }
                }
                yield return (new Vector2Int(0, 0), Color.clear);
            }
            else
            {
                yield return (new Vector2Int(0, 0), Color.clear);
            }
        }

        public Pixel[,] ToPixels(AsepriteReader reader, Vector2Int size, Aseprite aseprite)
        {
            var pixels = new Pixel[size.x, size.y];

            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    pixels[x, y] = Pixel.Deserialize(reader, aseprite);

            return pixels;
        }

        public uint[,] ToTiles(AsepriteReader reader, Vector2Int size)
        {
            var tiles = new uint[size.x, size.y];

            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    tiles[x, y] = reader.Dword();

            return tiles;
        }
    }
}
