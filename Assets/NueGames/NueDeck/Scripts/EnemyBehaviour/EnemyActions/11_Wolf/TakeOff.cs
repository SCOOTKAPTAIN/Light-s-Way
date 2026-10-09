using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class TakeOff : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.TakeOff;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            self.CharacterStats.ApplyStatus(StatusType.Flying, 1, self);

            PlayActionFx(actionParameters, self.transform, FxType.TakeOff);
            PlayActionAudio(actionParameters, AudioActionType.TakeOff);
        }
    }
}
