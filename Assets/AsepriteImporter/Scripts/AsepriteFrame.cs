using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public int[] chunkTypes;

        public static AsepriteFrame Deserialize(AsepriteReader reader)
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
                        break;
                    default:
                        break;
                }

                reader.Position = chunkEnd;
            }

            reader.Position = frameEnd;
            return frame;
        }
    }
}
