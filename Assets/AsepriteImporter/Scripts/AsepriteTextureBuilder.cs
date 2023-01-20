using Negi0109.AsepriteImporter.Aseprite;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Negi0109.AsepriteImporter.Editor
{
    public class AsepriteTextureBuilder {
        private Aseprite.Aseprite _aseprite;
        private Vector2Int _spriteSize;
        private FrameDirection _frameDirection;
        private bool _edging;

        public AsepriteTextureBuilder(Aseprite.Aseprite aseprite, Vector2Int spriteSize, FrameDirection frameDirection, bool edging) {
            _aseprite = aseprite;
            _spriteSize = spriteSize;
            _frameDirection = frameDirection;
            _edging = edging;
        }

        public Texture2D Build()
        {
            HashSet<int> activeLayers = new HashSet<int>(Enumerable.Range(0, _aseprite.layers.Count));

            return Build(activeLayers);
        }

        public Texture2D Build(HashSet<int> activeLayers)
        {
            var header = _aseprite.header;
            Texture2D tex;

            if (_frameDirection == FrameDirection.Vertical) {
                tex = new Texture2D(header.size.x, header.size.y * header.frames);

                for (int x = 0; x < tex.width; x++)
                    for (int y = 0; y < tex.height; y++)
                        tex.SetPixel(x, y, Color.clear);

                for (int i = 0; i < header.frames; i++)
                {
                    _aseprite.frames[i].GenerateTexture(_aseprite, tex, new Vector2Int(0, i * header.size.y), activeLayers);
                }

                tex.Apply();
            } else {
                tex = new Texture2D(header.size.x * header.frames, header.size.y);

                for (int x = 0; x < tex.width; x++)
                    for (int y = 0; y < tex.height; y++)
                        tex.SetPixel(x, y, Color.clear);

                for (int i = 0; i < header.frames; i++)
                {
                    _aseprite.frames[i].GenerateTexture(_aseprite, tex, new Vector2Int(i * header.size.x, 0), activeLayers);
                }

                tex.Apply();
            }

            if (_edging) tex = EdgingTexture(tex, _spriteSize);


            return tex;
        }

        private Texture2D EdgingTexture(Texture2D texture, Vector2Int spriteSize)
        {
            var sx = texture.width / spriteSize.x;
            var sy = texture.height / spriteSize.y;

            var tex = new Texture2D(sx * (spriteSize.x + 1) + 2, sy * (spriteSize.y + 1) + 2);
            tex.filterMode = FilterMode.Point;

            for (int x = 0; x < tex.width; x++)
                for (int y = 0; y < tex.height; y++)
                    tex.SetPixel(x, y, Color.clear);

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int dx = 0; dx < spriteSize.x; dx++)
                        for (int dy = 0; dy < spriteSize.y; dy++)
                            tex.SetPixel(
                                x * (spriteSize.x + 1) + dx + 1,
                                y * (spriteSize.y + 1) + dy + 1,
                                texture.GetPixel(
                                    x * spriteSize.x + dx,
                                    y * spriteSize.y + dy
                                )
                            );

            return tex;
        }
    }
}
