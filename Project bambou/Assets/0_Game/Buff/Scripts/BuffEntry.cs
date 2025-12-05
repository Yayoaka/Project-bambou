using Stats.Data;

namespace Buff
{
    public class BuffEntry
    {
        public StatType Stat;     // La stat affectée
        public float Amount;      // Valeur du buff (flat ou %)
        public bool IsPercentage; // True → % | False → flat
        public float ExpireTime;  // Quand il expire
    }
}