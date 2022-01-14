using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteHeader
    {
        public Types.AsepriteHeaderType type;

        public static AsepriteHeader Deserialize(byte[] bytes)
        {
            var header = new AsepriteHeader();
            Types.AsepriteHeaderType tmp = Types.AsepriteHeaderType.Deserialize(bytes);

            return header;
        }
    }
}
