using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class ApplyDebuffAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.ApplyDebuff;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var target = actionParameters.TargetCharacter;
            if (!target || actionParameters.ActionData == null || actionParameters.ActionData.StatusType == StatusType.None)
                return;

            target.CharacterStats.ApplyStatus(
                actionParameters.ActionData.StatusType,
                UnityEngine.Mathf.RoundToInt(actionParameters.Value));

            PlayActionFx(actionParameters, target.transform, FxType.Debuff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}