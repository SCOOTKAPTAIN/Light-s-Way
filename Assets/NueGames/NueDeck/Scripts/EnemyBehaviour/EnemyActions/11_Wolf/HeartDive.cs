using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class HeartDive : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.HeartDive;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null || actionParameters.TargetCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            var target = actionParameters.TargetCharacter;
            var damage = CalculateValue(actionParameters.Value, self, target, actionParameters.ActionData);

            target.CharacterStats.Damage(damage, false, "red", self);
            self.CharacterStats.ClearStatus(StatusType.Flying);

            PlayActionFx(actionParameters, target.transform, FxType.HeartDive);
            PlayActionAudio(actionParameters, AudioActionType.HeartDive);
        }
    }
}
