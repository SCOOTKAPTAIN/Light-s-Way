using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class SlowButPrecise : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.SlowButPrecise;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            self.CharacterStats.ApplyStatus(StatusType.Marksman, 3, self);

            PlayActionFx(actionParameters, self.transform, FxType.SlowButPrecise);
            PlayActionAudio(actionParameters, AudioActionType.SlowButPrecise);
        }
    }
}
