using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteFrameHeader
    {
        public int magicNumber;
        public int duration;
        public uint chunks;

        public static AsepriteFrameHeader Deserialize(AsepriteReader reader)
        {
            var frameHeader = new AsepriteFrameHeader();
            reader.Seek(4);
            frameHeader.magicNumber = reader.Word();
            reader.Seek(2);
            frameHeader.duration = reader.Word();
            reader.Seek(2);
            frameHeader.chunks = reader.Dword();

            return frameHeader;
        }
    }
}
