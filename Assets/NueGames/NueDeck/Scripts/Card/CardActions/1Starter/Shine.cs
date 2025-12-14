using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Shine: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Shine;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            
            var value =  GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            if (GameManager.PersistentGameplayData.light < 10)
            {
                CollectionManager.DrawCards(Mathf.RoundToInt(1));
                FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.NoLight);
                AudioManager.PlayOneShot(AudioActionType.NoLight);
                return;
            }

            CollectionManager.DrawCards(Mathf.RoundToInt(1));
            GameManager.PersistentGameplayData.ChangeLight(-10);
            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.Shine);
            
            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);


           
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}