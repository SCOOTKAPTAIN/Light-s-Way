using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Flash: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Flash;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            

            if (GameManager.PersistentGameplayData.light < 10)
            {
                CollectionManager.DrawCards(Mathf.RoundToInt(1));
                FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.NoLight);
                AudioManager.PlayOneShot(AudioActionType.NoLight);
                return;
            }

            CollectionManager.DrawCards(Mathf.RoundToInt(1));
            actionParameters.TargetCharacter.CharacterStats.ApplyStatus(StatusType.Stun, Mathf.RoundToInt(1));
            GameManager.PersistentGameplayData.ChangeLight(-10);
            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.Flash);
          
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}