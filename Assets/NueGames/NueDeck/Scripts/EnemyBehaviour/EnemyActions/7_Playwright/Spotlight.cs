using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Spotlight : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Spotlight;
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
            target.CharacterStats.ApplyStatus(StatusType.Spotlight, 1, actionParameters.SelfCharacter);

            PlayActionFx(actionParameters, target.transform, FxType.Spotlight);
            PlayActionAudio(actionParameters, AudioActionType.Spotlight);
        }
    }
}