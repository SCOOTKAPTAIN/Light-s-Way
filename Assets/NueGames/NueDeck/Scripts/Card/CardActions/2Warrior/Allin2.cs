using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Allin2 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.AllIn2;
        public override void DoAction(CardActionParameters actionParameters)
        {if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var strengthMultiplier = CardScaling.AddWisdom(3f);
            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + (selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue * strengthMultiplier);

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.AllIn2);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

           
            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.AllIn2);

        }
    }
}