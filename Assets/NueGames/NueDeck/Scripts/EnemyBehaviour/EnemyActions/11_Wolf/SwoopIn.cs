using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class SwoopIn : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.SwoopIn;
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
            var wasBlocked = target.CharacterStats.Damage(damage, false, "red", self);

            if (wasBlocked)
            {
                self.CharacterStats.ClearStatus(StatusType.Flying);
                if (!self.CharacterStats.StatusDict[StatusType.Chaotic].IsActive)
                    self.CharacterStats.ApplyStatus(StatusType.Stun, 1, self);
            }

            PlayActionFx(actionParameters, target.transform, FxType.SwoopIn);
            PlayActionAudio(actionParameters, AudioActionType.SwoopIn);
        }
    }
}
