using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UI
{
    public class TextureAtlasGenerator : EditorWindow //GG chat gpt 2 try
    {
        private DefaultAsset folder;
        private int padding = 0;
        private string outputName = "SpriteAtlas";

        [MenuItem("Tools/Generate Texture Atlas")]
        public static void ShowWindow()
        {
            GetWindow<TextureAtlasGenerator>("Atlas Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Generate Atlas From Folder", EditorStyles.boldLabel);

            folder = (DefaultAsset)EditorGUILayout.ObjectField("Source Folder", folder, typeof(DefaultAsset), false);
            padding = EditorGUILayout.IntField("Padding (px)", padding);
            outputName = EditorGUILayout.TextField("Output Name", outputName);

            if (GUILayout.Button("Generate Atlas"))
                GenerateAtlas();
        }

        private void GenerateAtlas()
        {
            if (folder == null)
            {
                Debug.LogError("❌ No folder selected.");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(folder);
            string[] files = Directory.GetFiles(folderPath);

            List<Texture2D> textures = new List<Texture2D>();

            foreach (string file in files)
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                {
                    Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(file);

                    if (source != null)
                    {
                        // Force un format lisible
                        string path = AssetDatabase.GetAssetPath(source);
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                        if (importer != null)
                        {
                            importer.isReadable = true;
                            importer.textureCompression = TextureImporterCompression.Uncompressed;
                            importer.SaveAndReimport();
                        }

                        textures.Add(source);
                    }
                }
            }

            // Create empty readable texture atlas in RGBA32
            Texture2D atlas = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);

            // Pack all textures
            Rect[] rects = atlas.PackTextures(textures.ToArray(), padding, 2048, false);

            // Encode to PNG
            byte[] png = atlas.EncodeToPNG();
            if (png == null)
            {
                Debug.LogError("❌ EncodeToPNG failed. Texture may not be readable.");
                return;
            }

            string outputPath = $"Assets/{outputName}.png";
            File.WriteAllBytes(outputPath, png);
            AssetDatabase.Refresh();

            Debug.Log($"✅ Atlas generated at: {outputPath}");
        }
    }
}
