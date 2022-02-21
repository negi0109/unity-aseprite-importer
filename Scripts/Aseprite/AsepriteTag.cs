using UnityEngine;

namespace Negi0109.AsepriteImporter.Aseprite
{
    public class Tag
    {
        public int from;
        public int to;
        public int loopAnimationDirection;
        public Color color;
        public string name;

        public static Tag Deserialize(AsepriteReader reader, Aseprite aseprite)
        {
            var tag = new Tag();
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
