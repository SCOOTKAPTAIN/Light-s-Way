using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Stigmata : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Stigmata;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.TargetCharacter == null || actionParameters.SelfCharacter == null)
                return;

            var target = actionParameters.TargetCharacter;
            var self = actionParameters.SelfCharacter;
            var damage = CalculateDamageValue(actionParameters.Value, self, target, actionParameters.ActionData);

            target.CharacterStats.Damage(damage, false, "red", self);
            target.CharacterStats.ApplyStatus(StatusType.Bleeding, 1, self);

            var cardToAdd = actionParameters.ActionData?.CardToAdd;
            if (CollectionManager != null && cardToAdd != null)
            {
                for (var index = 0; index < 3; index++)
                    CollectionManager.AddCardToDrawPileWithAnimation(cardToAdd);
            }

            PlayActionFx(actionParameters, target.transform, FxType.Stigmata);
            PlayActionAudio(actionParameters, AudioActionType.Stigmata);
        }
    }
}