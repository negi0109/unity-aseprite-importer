using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace Negi0109.AsepriteImporter.Aseprite
{
    public class AsepriteReader
    {
        public long Position
        {
            get => reader.BaseStream.Position;
            set => reader.BaseStream.Position = value;
        }

        public Stream BaseStream
        {
            get => reader.BaseStream;
        }

        private readonly BinaryReader reader;
        public AsepriteReader(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public byte Byte() => reader.ReadByte();
        public int Word() => reader.ReadUInt16();
        public int Short() => reader.ReadInt16();
        public uint Dword() => reader.ReadUInt32();
        public int Long() => reader.ReadInt32();
        public string String() => new string(new UTF8Encoding().GetChars(reader.ReadBytes(Word())));
        public void Seek(long count) => reader.BaseStream.Seek(count, SeekOrigin.Current);
    }
}
