using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.IO;

using Negi0109.AsepriteImporter;

namespace Tests
{
    public class ReadAsepriteBinaryTest
    {
        public static string currentPath = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
        public static string path = "/Fixtures/Sprite-0001.aseprite";
        public static Aseprite aseprite;

        [SetUp]
        public static void SetUp()
        {
            var bytes = File.ReadAllBytes(currentPath + path);
            aseprite = Aseprite.Deserialize(bytes);
        }

        [Test]
        public void AsepriteHeader()
        {
            var header = aseprite.header;

            Assert.That(header.magicNumber, Is.EqualTo(0xA5E0));
            Assert.That(header.size.x, Is.EqualTo(3));
            Assert.That(header.size.y, Is.EqualTo(4));
        }

        [Test]
        public void AsepriteFrameHeader()
        {
            var frameHeader = aseprite.frameHeader;

            Assert.That(frameHeader.magicNumber, Is.EqualTo(0xF1FA));
            Assert.That(frameHeader.chunks, Is.Not.EqualTo(0xFFFF));
        }
    }
}
