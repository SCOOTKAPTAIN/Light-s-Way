using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SharpDagger: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SharpDagger;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue
             + targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.SharpDagger);
            FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.Bleed);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Bleeding, 1);


            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}