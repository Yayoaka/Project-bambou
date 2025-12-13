using UnityEditor;
using UnityEngine;

namespace Wave.Editor
{
    [CustomEditor(typeof(WaveAsset))]
    public class WaveAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _actions;
        private SerializedProperty _onComplete;
        private SerializedProperty _nextWave;
        private SerializedProperty _nextGameState;

        private void OnEnable()
        {
            _actions = serializedObject.FindProperty("actions");
            _onComplete = serializedObject.FindProperty("onComplete");
            _nextWave = serializedObject.FindProperty("nextWave");
            _nextGameState = serializedObject.FindProperty("nextGameState");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawActions();
            DrawCompletion();

            serializedObject.ApplyModifiedProperties();
        }

        // --------------------------------------------------
        // ACTIONS
        // --------------------------------------------------
        private void DrawActions()
        {
            EditorGUILayout.LabelField("Wave Actions", EditorStyles.boldLabel);
            GUILayout.Space(4);

            for (int i = 0; i < _actions.arraySize; i++)
            {
                var action = _actions.GetArrayElementAtIndex(i);
                var type = action.FindPropertyRelative("type");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Action {i}", EditorStyles.boldLabel);

                if (GUILayout.Button("✕", GUILayout.Width(22)))
                {
                    _actions.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(type);

                var actionType = (WaveActionType)type.enumValueIndex;

                switch (actionType)
                {
                    case WaveActionType.Spawn:
                        DrawSpawnAction(action);
                        break;

                    case WaveActionType.WaitTime:
                        EditorGUILayout.PropertyField(
                            action.FindPropertyRelative("duration"),
                            new GUIContent("Duration (sec)")
                        );
                        break;

                    case WaveActionType.WaitUntilAliveCount:
                        EditorGUILayout.PropertyField(
                            action.FindPropertyRelative("aliveCount"),
                            new GUIContent("Alive Count ≤")
                        );
                        break;

                    case WaveActionType.WaitUntilClear:
                        EditorGUILayout.HelpBox(
                            "Wait until all enemies are dead",
                            MessageType.Info
                        );
                        break;
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            if (GUILayout.Button("+ Add Action"))
            {
                _actions.InsertArrayElementAtIndex(_actions.arraySize);
            }

            GUILayout.Space(12);
        }

        private static void DrawSpawnAction(SerializedProperty action)
        {
            EditorGUILayout.PropertyField(action.FindPropertyRelative("enemy"));
            EditorGUILayout.PropertyField(action.FindPropertyRelative("count"));
            EditorGUILayout.PropertyField(action.FindPropertyRelative("spawnInterval"));
            EditorGUILayout.PropertyField(action.FindPropertyRelative("startDelay"));

            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(
                action.FindPropertyRelative("pattern"),
                new GUIContent("Spawn Pattern")
            );
        }

        // --------------------------------------------------
        // COMPLETION / ENCHAINEMENT
        // --------------------------------------------------
        private void DrawCompletion()
        {
            EditorGUILayout.LabelField("On Wave Complete", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(_onComplete);

            var mode = (WaveCompletionAction)_onComplete.enumValueIndex;

            switch (mode)
            {
                case WaveCompletionAction.NextWave:
                    EditorGUILayout.PropertyField(
                        _nextWave,
                        new GUIContent("Next Wave")
                    );
                    break;

                case WaveCompletionAction.ChangeGameState:
                    EditorGUILayout.PropertyField(
                        _nextGameState,
                        new GUIContent("Next Game State")
                    );
                    break;
            }

            EditorGUILayout.EndVertical();
        }
    }
}