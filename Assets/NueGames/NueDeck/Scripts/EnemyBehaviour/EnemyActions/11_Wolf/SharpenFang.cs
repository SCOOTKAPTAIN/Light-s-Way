using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class SharpenFang : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.SharpenFang;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            var rend = CalculateValue(actionParameters.Value, self, self, actionParameters.ActionData);
            self.CharacterStats.ApplyStatus(StatusType.Rend, rend, self);

            PlayActionFx(actionParameters, self.transform, FxType.SharpenFang);
            PlayActionAudio(actionParameters, AudioActionType.SharpenFang);
        }
    }
}