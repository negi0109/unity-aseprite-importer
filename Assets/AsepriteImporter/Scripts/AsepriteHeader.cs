using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteHeader
    {
        public uint fileSize;
        public int magicNumber;
        public int frames;
        public Vector2Int size;
        public int colorDepth;
        public uint flags;
        public int speed;
        public int paletteEntry;
        public int colorNumber;
        public Vector2Int pixelSize;
        public Vector2Int gridPosition;
        public Vector2Int gridSize;

        public static AsepriteHeader Deserialize(AsepriteReader reader)
        {
            var header = new AsepriteHeader();

            header.fileSize = reader.Dword();
            header.magicNumber = reader.Word();
            if (header.magicNumber != 0xA5E0) Aseprite.AsepriteFormatError();

            header.frames = reader.Word();
            header.size.x = reader.Word();
            header.size.y = reader.Word();
            header.colorDepth = reader.Word();
            header.flags = reader.Dword();
            header.speed = reader.Word();
            reader.Seek(8);
            header.paletteEntry = reader.Byte();
            reader.Seek(3);
            header.colorNumber = reader.Word();
            header.pixelSize.x = reader.Byte();
            header.pixelSize.y = reader.Byte();
            header.gridPosition.x = reader.Short();
            header.gridPosition.y = reader.Short();
            header.gridSize.x = reader.Word();
            header.gridSize.y = reader.Word();
            reader.Seek(84);

            return header;
        }
    }
}
