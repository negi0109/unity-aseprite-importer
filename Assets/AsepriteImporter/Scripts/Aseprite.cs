using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Negi0109.AsepriteImporter
{
    public class Aseprite
    {
        public AsepriteHeader header;

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

            return aseprite;
        }
    }
}
