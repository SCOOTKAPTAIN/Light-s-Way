using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ForcedMonologue : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ForcedMonologue;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.TargetCharacter == null || actionParameters.SelfCharacter == null)
                return;

            for (var hit = 0; hit < actionParameters.RepeatCount; hit++)
            {
                var damageWasFullyBlocked = actionParameters.TargetCharacter.CharacterStats.Damage(
                    CalculateDamageValue(actionParameters.Value, actionParameters.SelfCharacter,
                        actionParameters.TargetCharacter, actionParameters.ActionData),
                    false, "red", actionParameters.SelfCharacter);

                if (damageWasFullyBlocked && actionParameters.ActionData?.CardToAdd != null)
                    CollectionManager?.AddCardToDrawPileWithAnimation(actionParameters.ActionData.CardToAdd);

                PlayActionFx(actionParameters, actionParameters.TargetCharacter.transform, FxType.ForcedMonologue);
                PlayActionAudio(actionParameters, AudioActionType.ForcedMonologue);
            }
        }
    }
}
