namespace Upgrades
{
    public interface IUpgradeComponent
    {
        // --- PASSIVES ---
        void AddPassive(PassiveUpgrades.Data.PassiveUpgradeData passive);

        // --- WEAPONS ---
        void AddWeapon(WeaponUpgrades.Data.WeaponUpgradeData weapon);

        // --- STAT ACCESS (déléguée à StatsComponent) ---
        float GetStat(Stats.Data.StatType statType);
    }
}