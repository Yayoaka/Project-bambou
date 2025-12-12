namespace Stats.Data
{
    public enum StatType
    {
        // --- OFFENSIVE ---
        AttackDamage,
        AbilityPower,
        AttackRange,

        // --- CRITICAL ---
        CritChance,
        CritMultiplier,

        // --- DEFENSIVE ---
        Armor,
        MagicResist,

        // --- RESOURCE (optional) ---
        MaxHealth,

        // --- SPEED (optional) ---
        AttackSpeed,
        Haste,
        MoveSpeed,

        // --- CUSTOM (expand later) ---
        ProjectileCount,
        ProjectileSize,
        ProjectileSpeedMultiplier,
    }
}