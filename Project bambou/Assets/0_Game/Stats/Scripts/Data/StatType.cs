namespace Stats.Data
{
    public enum StatType
    {
        // --- OFFENSIVE ---
        AttackDamage,
        AbilityPower,

        // --- CRITICAL ---
        CritChance,
        CritMultiplier,

        // --- DEFENSIVE ---
        Armor,
        MagicResist,
        DamageReduction,    // réduction flat (%)

        // --- RESOURCE (optional) ---
        MaxHealth,
        MaxMana,

        // --- SPEED (optional) ---
        AttackSpeed,
        Haste,
        MoveSpeed,

        // --- CUSTOM (expand later) ---
        LifeSteal,
        Tenacity,
        CooldownReduction,
        ProjectileCount,
        ProjectileSize,
        ProjectileSpeedMultiplier,
    }
}