using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteFrame
    {
        public enum ChunType
        {
            OldPalatte1 = 0x0004,
            OldPalatte2 = 0x0011,
            Layer = 0x2004,
            Cel = 0x2005,
            ColorProfile = 0x2007,
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
            frame.chunkCount = reader.Word();
            frame.duration = reader.Word();
            reader.Seek(2);
            reader.Seek(4);

            frame.chunkTypes = new int[frame.chunkCount];

            for (int i = 0; i < frame.chunkCount; i++)
            {
                var chunkEnd = reader.Position + reader.Dword();
                var chunkType = reader.Word();
                frame.chunkTypes[i] = chunkType;


                reader.Position = chunkEnd;
            }

            reader.Position = frameEnd;
            return frame;
        }
    }
}
