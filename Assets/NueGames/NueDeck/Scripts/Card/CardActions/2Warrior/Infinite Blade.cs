using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class InfiniteBlade: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.InfiniteBlade;
        public override void DoAction(CardActionParameters actionParameters)
        {
            
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.InfiniteBlade, new Vector3(-0.1f, 0.2f, 0f));

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
            CollectionManager.DrawCards(Mathf.RoundToInt(1));


           

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);

                
              
        }
    }
}