using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Desperation : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Desperation;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var target = actionParameters.SelfCharacter;
            if (target == null)
                return;

            target.CharacterStats.ApplyStatus(StatusType.Desperation, 1);

            PlayActionFx(actionParameters, target.transform, FxType.Desperation);
            PlayActionAudio(actionParameters, AudioActionType.Desperation);
        }
    }
}
