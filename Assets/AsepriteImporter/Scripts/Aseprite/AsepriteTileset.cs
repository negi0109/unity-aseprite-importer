using System;
using UnityEngine;
using System.IO.Compression;
using System.Collections.Generic;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class Tileset
    {
        public uint id;
        public TilesetFlag flags;
        public uint count;
        public Vector2Int size;
        public int width;
        public int height;

        public int baseIndex;
        public string name;

        public Tile[] tiles;

        public static Tileset Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var tileset = new Tileset();

            tileset.id = reader.Dword();
            tileset.flags = (TilesetFlag)reader.Dword();
            tileset.count = reader.Dword();

            tileset.size.x = reader.Word();
            tileset.size.y = reader.Word();

            tileset.baseIndex = reader.Short();

            reader.Seek(14); // Reserved
            tileset.name = reader.String();
            if (tileset.flags.HasFlag(TilesetFlag.ExternalFile))
            {
                reader.Dword(); // ID of the external file.
                reader.Dword(); // Tileset ID in the external file
            }
            if (tileset.flags.HasFlag(TilesetFlag.InsideThisFile))
            {
                reader.Dword(); // length

                reader.Seek(2); // zlib stream unnecessary parts

                var stream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
                tileset.tiles = new Tile[tileset.count];

                for (var i = 0; i < tileset.count; i++)
                {
                    var tile = tileset.tiles[i] = new Tile();
                    tile.pixels = ToPixels(
                        new AsepriteReader(stream),
                        tileset.size,
                        aseprite
                    );
                }
            }

            return tileset;
        }

        [Flags]
        public enum TilesetFlag
        {
            ExternalFile = 1,
            InsideThisFile = 2,
            Empty = 4,
            FlippedX = 8,
            SameYFlip = 16,
            SameDiagonalFlip = 32,
        }

        private static Pixel[,] ToPixels(AsepriteReader reader, Vector2Int size, Aseprite aseprite)
        {
            var pixels = new Pixel[size.x, size.y];

            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    pixels[x, y] = Pixel.Deserialize(reader, aseprite);

            return pixels;
        }

        public IEnumerable<(Vector2Int, Color)> GetTileColors(Aseprite aseprite, uint tileIndex)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var pixel = tiles[tileIndex].pixels[x, y];
                    var color = pixel.GetColor(aseprite);

                    yield return (new Vector2Int(x, y), color);
                }
            }
        }

        public class Tile
        {
            public Pixel[,] pixels;
        }
    }
}
