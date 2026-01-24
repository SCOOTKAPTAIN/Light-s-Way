using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ReturnToSender1: CardActionBase
    {
        private static readonly StatusType[] DebuffTypes = new[]
        {
            StatusType.Poison,
            StatusType.Stun,
            StatusType.Frozen,
            StatusType.Fragile,
            StatusType.Weak,
            StatusType.Bleeding,
            StatusType.Frostbite,
            StatusType.Burning,
            StatusType.NoDraw,
            StatusType.NoGainMana,
            StatusType.Judged,
            StatusType.Obscured
        };

        // Static dictionary to store cleared debuffs for ReturnToSender2
        public static Dictionary<StatusType, int> ClearedDebuffs { get; set; } = new Dictionary<StatusType, int>();

        public override CardActionType ActionType => CardActionType.ReturnToSender;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.SelfCharacter) return;

            var selfCharacter = actionParameters.SelfCharacter;

            // Clear the previous debuffs dictionary
            ClearedDebuffs.Clear();

            // Save debuff values and clear them
            foreach (var debuffType in DebuffTypes)
            {
                var debuffStatus = selfCharacter.CharacterStats.StatusDict[debuffType];
                if (debuffStatus.StatusValue > 0)
                {
                    // Save the debuff and its value
                    ClearedDebuffs[debuffType] = debuffStatus.StatusValue;
                    
                    // Clear the debuff from the player
                    selfCharacter.CharacterStats.ApplyStatus(debuffType, -debuffStatus.StatusValue);
                }
            }

            FxManager.PlayFxAtPosition(selfCharacter.transform.position, FxType.ReturnToSender);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
