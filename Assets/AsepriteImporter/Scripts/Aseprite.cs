using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Negi0109.AsepriteImporter
{
    public class Aseprite
    {
        public AsepriteHeader header;
        public AsepriteFrame[] frames;
        public Color[] palatte;

        public static Aseprite Deserialize(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return Deserialize(stream);
        }

        public static Aseprite Deserialize(Stream stream)
        {
            var aseprite = new Aseprite();
            var reader = new AsepriteReader(stream);

            aseprite.header = AsepriteHeader.Deserialize(reader);
            aseprite.palatte = new Color[255];
            aseprite.frames = new AsepriteFrame[aseprite.header.frames];

            for (int i = 0; i < aseprite.header.frames; i++)
                aseprite.frames[i] = AsepriteFrame.Deserialize(reader, aseprite);

            return aseprite;
        }

        public static void AsepriteFormatError() => throw new System.Exception("this file is not Aseprite format");

        // 実行結果確認用にエディタから呼び出すメソッド
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Aseprite-Debug/FirstFrame")]
        public static void GenerateFirstFrame()
        {
            Debug.Log("generate png");

            var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            var bytes = File.ReadAllBytes(path);
            var aseprite = Aseprite.Deserialize(bytes);
            var texture = aseprite.frames[0].GenerateTexture(aseprite);
            var png = texture.EncodeToPNG();
            File.WriteAllBytes(path + ".png", png);
        }

        [MenuItem("Assets/Create/Aseprite-Debug/FirstFrame", validate = true)]
        private static bool CanGenerateFirstFrame() => CheckAsepriteFile();

        public static bool CheckAsepriteFile()
        {
            var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            var extension = Path.GetExtension(path);

            return extension.Equals(".aseprite") || extension.Equals(".ase");
        }
#endif
    }
}
