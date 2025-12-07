namespace Effect
{
    namespace Stats.Data
    {
        public enum EffectModifierType
        {
            // Outgoing effects (caster)
            OutgoingDamage,
            OutgoingHeal,
            OutgoingShield,
            OutgoingDot,
            OutgoingHot,

            // Incoming (target)
            IncomingDamage,
            IncomingHeal,
            IncomingShield,

            // Utility
            MovementSpeed,
            AttackSpeed
        }
    }
}