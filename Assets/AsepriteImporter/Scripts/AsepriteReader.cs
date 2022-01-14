using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Negi0109.AsepriteImporter
{
    public class AsepriteReader
    {
        private BinaryReader reader;

        public AsepriteReader(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public byte Byte() => reader.ReadByte();
        public int Word() => reader.ReadUInt16();
        public int Short() => reader.ReadInt16();
        public uint Dword() => reader.ReadUInt32();
        public int Long() => reader.ReadInt32();
        public string String() => reader.ReadChars(Word()).ToString();
        public void Seek(int count) => reader.BaseStream.Seek((long)count, SeekOrigin.Current);

    }
}
