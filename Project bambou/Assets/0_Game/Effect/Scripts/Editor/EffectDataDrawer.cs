using UnityEditor;
using UnityEngine;

namespace Effect.Editor
{
    [CustomPropertyDrawer(typeof(EffectData))]
    public class EffectDataDrawer : PropertyDrawer
    {
        private const float Line = 18f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 1; // Kind

            var kind = (EffectKind)property.FindPropertyRelative("kind").enumValueIndex;

            switch (kind)
            {
                case EffectKind.Damage:
                    lines += 4; // baseValue, percentAD, percentAP, effectType
                    break;

                case EffectKind.Heal:
                    lines += 3; // baseValue, percentAP, isPercent
                    break;

                case EffectKind.Shield:
                    lines += 3; // baseValue, percentAP, isPercent
                    break;

                case EffectKind.Dot:
                case EffectKind.Hot:
                    lines += 6; // baseValue, AD, AP, duration, tick, effectType
                    break;

                case EffectKind.Buff:
                case EffectKind.Debuff:
                    lines += 4; // targetStat, baseValue, isPercent, duration
                    break;

                case EffectKind.Taunt:
                    lines += 1;
                    break;
            }

            return (Line + Spacing) * lines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float y = position.y;

            // Retrieve fields
            var kind      = property.FindPropertyRelative("kind");
            var baseValue = property.FindPropertyRelative("baseValue");
            var percentAD = property.FindPropertyRelative("percentAD");
            var percentAP = property.FindPropertyRelative("percentAP");
            var targetStat = property.FindPropertyRelative("targetStat");
            var isPercent = property.FindPropertyRelative("isPercent");
            var duration = property.FindPropertyRelative("duration");
            var tickDelay = property.FindPropertyRelative("tickDelay");
            var loop = property.FindPropertyRelative("loop");
            var effectType = property.FindPropertyRelative("effectType");

            // Draw K I N D
            Draw(property, position, ref y, kind);

            // Draw fields dynamically
            switch ((EffectKind)kind.enumValueIndex)
            {
                case EffectKind.Damage:
                    Draw(property, position, ref y, baseValue);
                    Draw(property, position, ref y, percentAD);
                    Draw(property, position, ref y, percentAP);
                    Draw(property, position, ref y, effectType);
                    break;

                case EffectKind.Heal:
                    Draw(property, position, ref y, baseValue);
                    Draw(property, position, ref y, percentAP);
                    Draw(property, position, ref y, isPercent);
                    break;

                case EffectKind.Shield:
                    Draw(property, position, ref y, baseValue);
                    Draw(property, position, ref y, percentAP);
                    Draw(property, position, ref y, isPercent);
                    break;

                case EffectKind.Dot:
                case EffectKind.Hot:
                    Draw(property, position, ref y, baseValue);
                    Draw(property, position, ref y, percentAD);
                    Draw(property, position, ref y, percentAP);
                    Draw(property, position, ref y, duration);
                    Draw(property, position, ref y, tickDelay);
                    Draw(property, position, ref y, effectType);
                    Draw(property, position, ref y, loop);
                    break;

                case EffectKind.Buff:
                case EffectKind.Debuff:
                    Draw(property, position, ref y, targetStat);
                    Draw(property, position, ref y, baseValue);
                    Draw(property, position, ref y, isPercent);
                    Draw(property, position, ref y, duration);
                    break;

                case EffectKind.Taunt:
                    Draw(property, position, ref y, duration);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private void Draw(SerializedProperty root, Rect pos, ref float y, SerializedProperty prop)
        {
            EditorGUI.PropertyField(
                new Rect(pos.x, y, pos.width, Line),
                prop,
                true
            );
            y += Line + Spacing;
        }
    }
}
