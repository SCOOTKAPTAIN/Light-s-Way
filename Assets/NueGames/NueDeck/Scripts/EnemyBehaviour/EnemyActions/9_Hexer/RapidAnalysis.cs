using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class RapidAnalysis : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.RapidAnalysis;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBlockValue(baseValue, selfCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var target = actionParameters.TargetCharacter != null
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            target.CharacterStats.ApplyStatus(
                StatusType.Block,
                CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, target, actionParameters.ActionData));

            var hexerStats = actionParameters.SelfCharacter.CharacterStats;
            if (hexerStats.StatusDict[StatusType.TotalAssimilation].IsActive)
                hexerStats.ApplyStatus(StatusType.Strength, 5);
            else
                hexerStats.ApplyStatus(StatusType.Assimilation, 5);

            hexerStats.ApplyStatus(StatusType.Assimilation, 3);

            PlayActionFx(actionParameters, target.transform, FxType.Block);
            PlayActionAudio(actionParameters, AudioActionType.Block);
        }
    }
}