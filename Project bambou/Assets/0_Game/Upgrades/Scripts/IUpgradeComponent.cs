namespace Upgrades
{
    public interface IUpgradeComponent
    {
        // --- PASSIVES ---
        void AddPassive(PassiveUpgrades.Data.PassiveUpgradeData passive);
        void UpgradePassive(PassiveUpgrades.Data.PassiveUpgradeData passive, int level);

        // --- WEAPONS ---
        void AddWeapon(WeaponUpgrades.Data.WeaponUpgradeData weapon);
        void UpgradeWeapon(WeaponUpgrades.Data.WeaponUpgradeData weapon, int level);

        // --- STAT ACCESS (déléguée à StatsComponent) ---
        (float, float) GetStat(Stats.Data.StatType statType);
    }
}