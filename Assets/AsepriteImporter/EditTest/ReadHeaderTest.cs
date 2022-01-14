using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.IO;

using Negi0109.AsepriteImporter;

namespace Tests
{
    public class ReadHeaderTest
    {
        public string currentPath = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
        public string path = "/Fixtures/Sprite-0001.aseprite";

        // A Test behaves as an ordinary method
        [Test]
        public void ReadMagicNumber()
        {
            // Use the Assert class to test conditions
            var bytes = File.ReadAllBytes(currentPath + path);
            var header = Aseprite.Deserialize(bytes).header;

            Assert.That((int)header.magicNumber, Is.EqualTo(0xA5E0));
        }

        [Test]
        public void ReadData()
        {
            var bytes = File.ReadAllBytes(currentPath + path);
            var header = Aseprite.Deserialize(bytes).header;

            Assert.That(header.size.x, Is.EqualTo(3));
            Assert.That(header.size.y, Is.EqualTo(4));
        }
    }
}
