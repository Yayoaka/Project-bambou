using UnityEditor;
using UnityEngine;

namespace Effect.Editor
{
    [CustomPropertyDrawer(typeof(EffectCastData))]
    public class EffectCastDataDrawer : PropertyDrawer
    {
        private const float SPACING = 3f;
        private float Line() => EditorGUIUtility.singleLineHeight + SPACING;

        private Rect Indent(Rect r, int level = 1)
        {
            float indent = 15f * level;
            return new Rect(r.x + indent, r.y, r.width - indent, r.height);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 0f;

            // cast mode
            h += Line();

            // targetSearchRange for directional modes
            var mode = (CastMode)property.FindPropertyRelative("castMode").enumValueIndex;
            if (mode == CastMode.ToClosestEnemy ||
                mode == CastMode.ToAnyTarget ||
                mode == CastMode.OnClosestEnemy ||
                mode == CastMode.OnAnyTarget)
                h += Line();

            // Projectile section
            h += Line();
            if (property.FindPropertyRelative("spawnProjectile").boolValue)
            {
                h += Line();   // prefab
                h += Line();   // destroyOnHit
                h += Line();   // projectileSpeed
                h += Line();   // projectileLifetime
                h += Line();   // projectileCount
            }

            // Zone section
            h += Line();
            if (property.FindPropertyRelative("spawnZone").boolValue)
            {
                h += Line();   // prefab
                h += Line();   // firstTickDelay
                h += Line();   // loop
                if (property.FindPropertyRelative("loop").boolValue)
                    h += Line(); // tickRate

                h += Line();   // zoneLifetime
                h += Line();   // zoneCount
                h += Line();   // followCaster
            }

            // Applied effects
            h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("appliedEffects"), true);

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect r = position;
            r.height = EditorGUIUtility.singleLineHeight;

            // -----------------------------------------------------
            // CAST MODE
            // -----------------------------------------------------
            var modeProp = property.FindPropertyRelative("castMode");
            EditorGUI.PropertyField(r, modeProp);
            r.y += Line();

            var mode = (CastMode)modeProp.enumValueIndex;

            if (mode == CastMode.ToClosestEnemy ||
                mode == CastMode.ToAnyTarget ||
                mode == CastMode.OnClosestEnemy ||
                mode == CastMode.OnAnyTarget)
            {
                EditorGUI.PropertyField(r, property.FindPropertyRelative("targetSearchRange"));
                r.y += Line();
            }

            // -----------------------------------------------------
            // PROJECTILE SECTION
            // -----------------------------------------------------
            var spawnProj = property.FindPropertyRelative("spawnProjectile");
            EditorGUI.PropertyField(r, spawnProj);
            r.y += Line();

            if (spawnProj.boolValue)
            {
                Rect ir = Indent(r);

                EditorGUI.PropertyField(ir, property.FindPropertyRelative("projectilePrefab"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("destroyOnHit"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("projectileSpeed"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("projectileLifetime"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("additionalProjectileCount"));
                r.y += Line();
            }

            // -----------------------------------------------------
            // ZONE SECTION
            // -----------------------------------------------------
            var spawnZone = property.FindPropertyRelative("spawnZone");
            EditorGUI.PropertyField(r, spawnZone);
            r.y += Line();

            if (spawnZone.boolValue)
            {
                Rect ir = Indent(r);

                EditorGUI.PropertyField(ir, property.FindPropertyRelative("zonePrefab"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("firstTickDelay"));
                r.y += Line();

                var loopProp = property.FindPropertyRelative("loop");
                ir = Indent(r);
                EditorGUI.PropertyField(ir, loopProp);
                r.y += Line();

                if (loopProp.boolValue)
                {
                    ir = Indent(r, 2);
                    EditorGUI.PropertyField(ir, property.FindPropertyRelative("tickRate"));
                    r.y += Line();
                }

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("zoneLifetime"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("additionalZoneCount"));
                r.y += Line();

                ir = Indent(r);
                EditorGUI.PropertyField(ir, property.FindPropertyRelative("followCaster"));
                r.y += Line();
            }

            // -----------------------------------------------------
            // APPLIED EFFECTS
            // -----------------------------------------------------
            EditorGUI.PropertyField(r, property.FindPropertyRelative("appliedEffects"), true);

            EditorGUI.EndProperty();
        }
    }
}
