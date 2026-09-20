using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Pound : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Pound;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter)
                return;

            var value = CalculateValue(
                actionParameters.Value,
                actionParameters.SelfCharacter,
                actionParameters.TargetCharacter,
                actionParameters.ActionData);

            actionParameters.TargetCharacter.CharacterStats.Damage(
                value,
                false,
                "red",
                actionParameters.SelfCharacter);

            var selfStats = actionParameters.SelfCharacter?.CharacterStats;
            if (selfStats != null && selfStats.StatusDict[StatusType.Chaotic].IsActive)
                CollectionManager?.ExhaustRandomCard();

            PlayActionFx(actionParameters, actionParameters.TargetCharacter.transform, FxType.Pound, new Vector3(0.2f,0.2f,0f));
            PlayActionAudio(actionParameters, AudioActionType.Pound);
        }
    }
}
