using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ApplyBuffAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ApplyBuff;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var target = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            if (!target || actionParameters.ActionData == null || actionParameters.ActionData.StatusType == StatusType.None)
                return;

            target.CharacterStats.ApplyStatus(
                actionParameters.ActionData.StatusType,
                UnityEngine.Mathf.RoundToInt(actionParameters.Value));

            PlayActionFx(actionParameters, target.transform, FxType.Buff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}