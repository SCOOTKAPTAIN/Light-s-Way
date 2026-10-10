using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class JellyCarapace : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.JellyCarapace;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var self = actionParameters.SelfCharacter;
            var brushOff = CalculateValue(actionParameters.Value, self, self, actionParameters.ActionData);
            self.CharacterStats.ApplyStatus(StatusType.BrushOff, brushOff, self);
            self.CharacterStats.HealWithPopup(Mathf.Max(1, Mathf.CeilToInt(self.CharacterStats.MaxHealth * 0.05f)));

            PlayActionFx(actionParameters, self.transform, FxType.JellyCarapace);
            PlayActionAudio(actionParameters, AudioActionType.JellyCarapace);
        }
    }
}
