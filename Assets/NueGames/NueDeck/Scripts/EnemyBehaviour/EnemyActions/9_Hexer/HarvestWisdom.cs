using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class HarvestWisdom : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.HarvestWisdom;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null || actionParameters.TargetCharacter == null)
                return;

            var target = actionParameters.TargetCharacter;
            var value = CalculateValue(
                actionParameters.Value,
                actionParameters.SelfCharacter,
                target,
                actionParameters.ActionData);

            target.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);
            var hexerStats = actionParameters.SelfCharacter.CharacterStats;

            if (hexerStats.StatusDict[StatusType.TotalAssimilation].IsActive)
                target.CharacterStats.ApplyPermanentProficiencyReduction(2);
            else
                target.CharacterStats.ApplyStatus(StatusType.Amnesia, 2, actionParameters.SelfCharacter);

            hexerStats.ApplyStatus(StatusType.Assimilation, 3);

            PlayActionFx(actionParameters, target.transform, FxType.Debuff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}