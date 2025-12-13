using UnityEditor;
using UnityEngine;

namespace Wave.Patterns.Editor
{
    [CustomEditor(typeof(GridSpawnPattern))]
    public class GridSpawnPatternEditor : UnityEditor.Editor
    {
        private static GUIStyle _cellStyleActive;
        private static GUIStyle _cellStyleInactive;
        
        private void InitStyles()
        {
            if (_cellStyleActive != null)
                return;

            _cellStyleActive = new GUIStyle(GUI.skin.button);
            _cellStyleActive.normal.background = MakeTex(1, 1, new Color(0.2f, 0.8f, 0.3f));
            _cellStyleActive.hover.background  = MakeTex(1, 1, new Color(0.3f, 0.9f, 0.4f));
            _cellStyleActive.active.background = MakeTex(1, 1, new Color(0.1f, 0.6f, 0.2f));

            _cellStyleInactive = new GUIStyle(GUI.skin.button);
            _cellStyleInactive.normal.background = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f));
            _cellStyleInactive.hover.background  = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f));
            _cellStyleInactive.active.background = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f));
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }
        
        public override void OnInspectorGUI()
        {
            var pattern = (GridSpawnPattern)target;

            pattern.EnsureMask();

            EditorGUI.BeginChangeCheck();

            pattern.width = EditorGUILayout.IntSlider("Width", pattern.width, 1, 20);
            pattern.height = EditorGUILayout.IntSlider("Height", pattern.height, 1, 20);
            pattern.spacing = EditorGUILayout.FloatField("Spacing", pattern.spacing);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Enable All"))
            {
                Undo.RecordObject(pattern, "Enable All Cells");
                pattern.SetAll(true);
                EditorUtility.SetDirty(pattern);
            }

            if (GUILayout.Button("Disable All"))
            {
                Undo.RecordObject(pattern, "Disable All Cells");
                pattern.SetAll(false);
                EditorUtility.SetDirty(pattern);
            }

            if (GUILayout.Button("Invert"))
            {
                Undo.RecordObject(pattern, "Invert Cells");
                pattern.Invert();
                EditorUtility.SetDirty(pattern);
            }

            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Mask", EditorStyles.boldLabel);

            for (int y = pattern.height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < pattern.width; x++)
                {
                    var value = pattern.GetCell(x, y);
                    InitStyles();

                    var content = value
                        ? new GUIContent(" ")
                        : new GUIContent("✕");

                    var style = value
                        ? _cellStyleActive
                        : _cellStyleInactive;

                    var newValue = GUILayout.Toggle(
                        value,
                        content,
                        style,
                        GUILayout.Width(28),
                        GUILayout.Height(28)
                    );

                    if (newValue != value)
                        pattern.SetCell(x, y, newValue);

                    if (newValue != value)
                        pattern.SetCell(x, y, newValue);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(pattern);
        }
    }
}