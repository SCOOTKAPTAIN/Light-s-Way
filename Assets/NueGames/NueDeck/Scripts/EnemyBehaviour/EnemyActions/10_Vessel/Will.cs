using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Will : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Will;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBlockValue(baseValue, selfCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var self = actionParameters.SelfCharacter;
            if (self == null)
                return;

            var block = CalculateValue(actionParameters.Value, self, self, actionParameters.ActionData);
            self.CharacterStats.ApplyStatus(StatusType.Block, block);
            self.CharacterStats.ApplyStatus(StatusType.Armor, 1);
            self.CharacterStats.ReduceDebuffStacks(3);

            PlayActionFx(actionParameters, self.transform, FxType.Block);
            PlayActionAudio(actionParameters, AudioActionType.Block);
        }
    }
}