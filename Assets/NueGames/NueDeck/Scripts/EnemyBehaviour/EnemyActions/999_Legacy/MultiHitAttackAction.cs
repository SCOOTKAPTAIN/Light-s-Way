using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class MultiHitAttackAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.MultiHitAttack;
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
                var value = CalculateValue(
                    actionParameters.Value,
                    actionParameters.SelfCharacter,
                    actionParameters.TargetCharacter,
                    actionParameters.ActionData);

                actionParameters.TargetCharacter.CharacterStats.Damage(
                    value,
                    false,
                    "red",
                    actionParameters.SelfCharacter);

                PlayActionFx(actionParameters, actionParameters.TargetCharacter.transform, FxType.Attack);
                PlayActionAudio(actionParameters, AudioActionType.Attack);
            }
        }
    }
}
