using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Negi0109.AsepriteImporter
{
    public class AsepriteLayer
    {
        public enum Flag
        {
            Visible = 1,
            Editable = 2,
            Lock = 4,
            Background = 8,
            LinkedCels = 16,
            LayerGroupShouldDisplayedCollapsed = 32,
            Reference = 64,
        }
        public enum Type
        {
            Normal = 0,
            Group = 1,
            Tilemap = 2,
        }

        public Flag flag;
        public Type type;
        public int childLevel;
        public Vector2Int size;
        public int blendMode;
        public float opacity;
        public string name;
        public uint tilesetIndex;

        public static AsepriteLayer Deserialize(AsepriteReader reader)
        {
            var layer = new AsepriteLayer();
            layer.flag = (Flag)reader.Word();
            layer.type = (Type)reader.Word();
            layer.childLevel = reader.Word();
            layer.size.x = reader.Word();
            layer.size.y = reader.Word();
            layer.blendMode = reader.Word();
            layer.opacity = reader.Byte() / 255f;
            reader.Seek(3);
            layer.name = reader.String();

            if (layer.type == Type.Tilemap)
                layer.tilesetIndex = reader.Dword();

            return layer;
        }
    }
}
