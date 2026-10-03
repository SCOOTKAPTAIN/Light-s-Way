using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GuidingLight: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GuidingLight;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            // Calculate damage
            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.GuidingLight);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            // Apply 1 + Potency Fragile for every 20 Light
            var currentLight = GameManager.PersistentGameplayData.light;
            var fragileStacks = Mathf.FloorToInt(currentLight / 20f) *
                Mathf.RoundToInt(CardScaling.AddPotency(1f));
            
            if (fragileStacks > 0)
            {
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Fragile, fragileStacks, selfCharacter);
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
