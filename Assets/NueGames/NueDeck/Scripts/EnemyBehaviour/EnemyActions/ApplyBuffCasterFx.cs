using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ApplyBuffCasterFx : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ApplyBuffCasterFx;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var target = actionParameters.TargetCharacter != null
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (target == null || actionParameters.SelfCharacter == null ||
                actionParameters.ActionData == null ||
                actionParameters.ActionData.StatusType == StatusType.None)
                return;

            target.CharacterStats.ApplyStatus(
                actionParameters.ActionData.StatusType,
                CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, target, actionParameters.ActionData));

            PlayActionFx(actionParameters, actionParameters.SelfCharacter.transform, FxType.Buff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}