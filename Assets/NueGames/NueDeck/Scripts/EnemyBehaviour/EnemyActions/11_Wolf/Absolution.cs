using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Absolution : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Absolution;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null || actionParameters.TargetCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            var target = actionParameters.TargetCharacter;
            var necrosis = self.CharacterStats.StatusDict[StatusType.Necrosis].StatusValue;

            if (necrosis > 0)
            {
                target.CharacterStats.ApplyStatus(StatusType.Necrosis, necrosis, self);
                self.CharacterStats.ClearStatus(StatusType.Necrosis);
            }

            PlayActionFx(actionParameters, target.transform, FxType.Absolution);
            PlayActionAudio(actionParameters, AudioActionType.Absolution);
        }
    }
}
