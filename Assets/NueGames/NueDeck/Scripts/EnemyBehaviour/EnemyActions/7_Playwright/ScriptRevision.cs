using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ScriptRevision : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ScriptRevision;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (CombatManager == null || CombatManager.CurrentEnemiesList == null)
                return;

            var cleansedAllyCount = 0;
            foreach (var ally in CombatManager.CurrentEnemiesList)
            {
                if (ally == null || ally.CharacterStats == null)
                    continue;

                var stats = ally.CharacterStats;
                if (!stats.StatusDict.TryGetValue(StatusType.SeveredString, out var severedString))
                    continue;

                if (!severedString.IsActive || severedString.StatusValue <= 0)
                    continue;

                cleansedAllyCount++;
                stats.ClearStatus(StatusType.SeveredString);
                stats.Heal(stats.MaxHealth);
            }

            if (cleansedAllyCount > 0 && actionParameters?.SelfCharacter != null)
            {
                var casterStats = actionParameters.SelfCharacter.CharacterStats;
                var healthLoss = UnityEngine.Mathf.CeilToInt(casterStats.MaxHealth * 0.10f * cleansedAllyCount);
                casterStats.Damage(healthLoss, true, "red", null);

                PlayActionFx(actionParameters, actionParameters.SelfCharacter.transform, FxType.Guard);
                PlayActionAudio(actionParameters, AudioActionType.Power);
            }
        }
    }
}