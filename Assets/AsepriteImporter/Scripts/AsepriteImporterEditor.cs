using UnityEditor.AssetImporters;
using UnityEditor;
using System.IO;

namespace Negi0109.AsepriteImporter
{
    [CustomEditor(typeof(AsepriteImporter))]
    public class AsepriteImporterEditor : ScriptedImporterEditor
    {
        private Aseprite aseprite;

        public void Reload()
        {
            var importer = target as AssetImporter;
            var bytes = File.ReadAllBytes(importer.assetPath);
            aseprite = Aseprite.Deserialize(bytes);
        }

        public override void OnEnable()
        {
            if (aseprite == null) Reload();
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            if (aseprite == null) Reload();

            EditorGUILayout.LabelField("Layers");
            EditorGUILayout.BeginVertical();

            var childLevel = 0;
            foreach (var layer in aseprite.layers)
            {
                EditorGUI.indentLevel += layer.childLevel - childLevel;
                EditorGUILayout.LabelField(layer.name);

                childLevel = EditorGUI.indentLevel;
            }
            EditorGUILayout.EndVertical();

            ApplyRevertGUI();
        }
    }
}
