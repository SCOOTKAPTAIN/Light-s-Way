using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ThoughtLeech : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ThoughtLeech;
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
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, target, actionParameters.ActionData);
            target.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

            if (actionParameters.SelfCharacter.CharacterStats.StatusDict[StatusType.TotalAssimilation].IsActive)
                target.CharacterStats.ApplyPermanentMaxManaReduction(1);
            else
                target.CharacterStats.ApplyStatus(StatusType.ManaDrain, 1, actionParameters.SelfCharacter);

            actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.Assimilation, 3);

            PlayActionFx(actionParameters, target.transform, FxType.Debuff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}