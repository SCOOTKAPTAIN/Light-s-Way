using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FinalGrace: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FinalGrace;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            // Play FX at position instead of on transform to prevent FX being destroyed when enemy dies
            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.FinalGrace);
              

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}