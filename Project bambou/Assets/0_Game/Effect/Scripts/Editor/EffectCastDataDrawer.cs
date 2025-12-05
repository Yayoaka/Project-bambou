using UnityEditor;
using UnityEngine;

namespace Effect.Editor
{
    [CustomPropertyDrawer(typeof(EffectCastData))]
    public class EffectCastDataDrawer : PropertyDrawer
    {
        private const float SPACING = 3f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 0f;

            // Direction mode
            h += Line();

            // targetSearchRange if needed
            var mode = (CastDirectionMode)property
                .FindPropertyRelative("castDirectionMode")
                .enumValueIndex;

            if (mode == CastDirectionMode.ToClosestEnemy || mode == CastDirectionMode.ToAnyTarget)
                h += Line();

            // Projectile
            h += Line(); // toggle

            if (property.FindPropertyRelative("spawnProjectile").boolValue)
            {
                h += Line(); // prefab
                h += Line(); // onCursor
            }

            // Zone
            h += Line(); // toggle

            if (property.FindPropertyRelative("spawnZone").boolValue)
            {
                h += Line(); // prefab
                h += Line(); // onCursor
                h += Line(); // followCaster
            }

            // Applied effects list
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("appliedEffects"), true);

            return h;

            float Line() => EditorGUIUtility.singleLineHeight + SPACING;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;

            // ---------------------------------------------
            // Direction Mode
            // ---------------------------------------------
            var modeProp = property.FindPropertyRelative("castDirectionMode");
            EditorGUI.PropertyField(r, modeProp);
            r.y += r.height + SPACING;

            var mode = (CastDirectionMode)modeProp.enumValueIndex;

            // Show range if needed
            if (mode == CastDirectionMode.ToClosestEnemy || mode == CastDirectionMode.ToAnyTarget)
            {
                EditorGUI.PropertyField(r, property.FindPropertyRelative("targetSearchRange"));
                r.y += r.height + SPACING;
            }

            // ---------------------------------------------
            // Projectile
            // ---------------------------------------------
            var spawnProj = property.FindPropertyRelative("spawnProjectile");
            EditorGUI.PropertyField(r, spawnProj);
            r.y += r.height + SPACING;

            if (spawnProj.boolValue)
            {
                EditorGUI.PropertyField(r, property.FindPropertyRelative("projectilePrefab"));
                r.y += r.height + SPACING;

                EditorGUI.PropertyField(r, property.FindPropertyRelative("onCursor"));
                r.y += r.height + SPACING;
            }

            // ---------------------------------------------
            // Zone
            // ---------------------------------------------
            var spawnZone = property.FindPropertyRelative("spawnZone");
            EditorGUI.PropertyField(r, spawnZone);
            r.y += r.height + SPACING;

            if (spawnZone.boolValue)
            {
                EditorGUI.PropertyField(r, property.FindPropertyRelative("zonePrefab"));
                r.y += r.height + SPACING;

                EditorGUI.PropertyField(r, property.FindPropertyRelative("onCursor"));
                r.y += r.height + SPACING;

                EditorGUI.PropertyField(r, property.FindPropertyRelative("followCaster"));
                r.y += r.height + SPACING;
            }

            // ---------------------------------------------
            // Effects list
            // ---------------------------------------------
            EditorGUI.PropertyField(r, property.FindPropertyRelative("appliedEffects"), new GUIContent("Applied Effects"), true);

            EditorGUI.EndProperty();
        }
    }
}
