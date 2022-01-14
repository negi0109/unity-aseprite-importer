// refs. https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Negi0109.AsepriteImporter.Types
{
    using Byte = System.Byte;
    using Word = System.UInt16;
    using Short = System.Int16;
    using Dword = System.UInt32;
    using Long = System.Int32;
    using Fixed = System.Int32;

    public unsafe struct AsepriteHeaderType
    {
        public Dword fileSize;
        public Word magicNumber;
        public Word frames;
        public Word width;
        public Word height;
        public Word colorDepth;
        public Dword flags;
        public Word speed;
        public Word zero1;
        public Word zero2;
        public Byte paletteEntry;
        public fixed Byte ignoredBytes[3];
        public Word colorNumber;
        public Byte pixelWidth;
        public Byte pixelHeight;
        public Short xPosition;
        public Short yPosition;
        public Word gridWidth;
        public Word gridHeight;
        public fixed Byte featureBytes[84];

        public static AsepriteHeaderType Deserialize(byte[] bytes)
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            AsepriteHeaderType data = (AsepriteHeaderType)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(AsepriteHeaderType));
            gch.Free();

            return data;
        }
    }
}
