using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Negi0109.AsepriteImporter
{
    public class Aseprite
    {
        public AsepriteHeader header;
        public AsepriteFrame[] frames;

        public static Aseprite Deserialize(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return Deserialize(stream);
        }

        public static Aseprite Deserialize(Stream stream)
        {
            var aseprite = new Aseprite();
            var reader = new AsepriteReader(stream);

            aseprite.header = AsepriteHeader.Deserialize(reader);
            aseprite.frames = new AsepriteFrame[aseprite.header.frames];

            for (int i = 0; i < aseprite.header.frames; i++)
                aseprite.frames[i] = AsepriteFrame.Deserialize(reader);

            return aseprite;
        }

        public static void AsepriteFormatError() => throw new System.Exception("this file is not Aseprite format");
    }
}
