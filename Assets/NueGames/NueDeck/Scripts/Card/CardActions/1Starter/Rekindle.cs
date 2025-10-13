using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Rekindle: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Rekindle;
        public override void DoAction(CardActionParameters actionParameters)
        {
            
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            

            if (GameManager.PersistentGameplayData.light < 10)
            {
                CollectionManager.DrawCards(Mathf.RoundToInt(1));
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.NoLight);
                AudioManager.PlayOneShot(AudioActionType.NoLight);
                return;
            }

            CollectionManager.DrawCards(Mathf.RoundToInt(3));
            CombatManager.IncreaseMana(Mathf.RoundToInt(GameManager.PersistentGameplayData.MaxMana));
            GameManager.PersistentGameplayData.ChangeLight(-10);
            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.Rekindle);
          
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}