using System.IO;
using UnityEditor.AssetImporters;

namespace Negi0109.AsepriteImporter
{
    [ScriptedImporter(0, "aseprite")]
    public class AsepriteImporter : ScriptedImporter
    {
        public Aseprite aseprite;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var tmp = Aseprite.Deserialize(bytes);

            // aseprite = tmp;
        }
    }
}
