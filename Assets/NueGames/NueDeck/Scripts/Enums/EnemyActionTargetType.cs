namespace NueGames.NueDeck.Scripts.Enums
{
    public enum EnemyActionTargetType
    {
        NoRestriction,  // Can target self or allies (random single target)
        SelfOnly,       // Can only target self
        AlliesOnly,     // Can only target allies, not self (random single target)
        AllAllies       // Targets all allies including self (AOE)
    }
}
