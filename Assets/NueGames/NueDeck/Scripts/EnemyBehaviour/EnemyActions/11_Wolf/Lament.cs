using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Lament : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Lament;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return selfCharacter == null
                ? 0
                : Mathf.Max(1, Mathf.CeilToInt(selfCharacter.CharacterStats.MaxHealth * 0.10f));
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            var necrosis = CalculateValue(actionParameters.Value, self, self, actionParameters.ActionData);
            self.CharacterStats.ApplyStatus(StatusType.Necrosis, necrosis, self);

            PlayActionFx(actionParameters, self.transform, FxType.Lament);
            PlayActionAudio(actionParameters, AudioActionType.Lament);
        }
    }
}
