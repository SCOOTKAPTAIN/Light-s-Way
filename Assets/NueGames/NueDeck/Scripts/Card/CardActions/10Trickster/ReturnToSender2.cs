using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ReturnToSender2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ReturnToSender2;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;

            // Apply all cleared debuffs to the target
            foreach (var debuff in ReturnToSender1.ClearedDebuffs)
            {
                targetCharacter.CharacterStats.ApplyStatus(debuff.Key, debuff.Value);
            }

            FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.ReturnToSender2);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.ReturnToSender2);
        }
    }
}
