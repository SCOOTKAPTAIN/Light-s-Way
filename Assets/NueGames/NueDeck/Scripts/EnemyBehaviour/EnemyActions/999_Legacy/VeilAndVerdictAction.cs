using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class VeilAndVerdictAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.VeilAndVerdict;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null || actionParameters.TargetCharacter == null)
                return;

            var blindDuration = CalculateValue(
                actionParameters.Value,
                actionParameters.SelfCharacter,
                actionParameters.TargetCharacter,
                actionParameters.ActionData);

            // Obscured is the gameplay status represented as Blind: it hides the player's cards.
            actionParameters.TargetCharacter.CharacterStats.ApplyStatus(StatusType.Obscured, blindDuration);
            actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.Ambush, 1);

            PlayActionFx(actionParameters, actionParameters.TargetCharacter.transform, FxType.Debuff);
           // PlayActionFx(actionParameters, actionParameters.SelfCharacter.transform, FxType.Buff);
            PlayActionAudio(actionParameters, AudioActionType.Power);
        }
    }
}
