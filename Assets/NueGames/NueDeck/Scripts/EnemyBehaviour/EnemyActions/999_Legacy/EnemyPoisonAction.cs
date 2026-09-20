using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyPoisonAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Poison;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            if (!newTarget) return;
            
            var value = CalculateValue(actionParameters.Value, selfCharacter, newTarget, actionParameters.ActionData);
            newTarget.CharacterStats.ApplyStatus(StatusType.Slimed, value);
            newTarget.CharacterStats.ApplyStatus(StatusType.Burden, value);
            newTarget.CharacterStats.ApplyStatus(StatusType.CloggedCircuits, value);
            newTarget.CharacterStats.ApplyStatus(StatusType.ManaDrain, value);

           
            // Apply Sabotaged effect (deals damage to self, then reduces Sabotaged by 1)
           // NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplySabotaged(selfCharacter);
            
            PlayActionFx(actionParameters, newTarget.transform, FxType.Poison);
            PlayActionAudio(actionParameters, AudioActionType.Poison);
        }
    }
}