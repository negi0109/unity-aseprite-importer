using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteTag
    {
        public int from;
        public int to;
        public int loopAnimationDirection;
        public Color color;
        public string name;

        public static AsepriteTag Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var tag = new AsepriteTag();
            tag.from = reader.Word();
            tag.to = reader.Word();
            tag.loopAnimationDirection = reader.Byte();
            reader.Seek(8);
            tag.color = new Color(reader.Byte() / 255f, reader.Byte() / 255f, reader.Byte() / 255f);
            reader.Seek(1);
            tag.name = reader.String();

            return tag;
        }
    }
}
