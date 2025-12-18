using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class StartingGear : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.StartingGear;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

           int roll = Random.Range(0, 4); // 0–3

           switch (roll)
           {
               case 0:
                   newTarget.CharacterStats.ApplyStatus(StatusType.Armor, 1);
                   break;
           
               case 1:
                   newTarget.CharacterStats.ApplyStatus(StatusType.Fortitude, 2);
                   break;

               case 2:
                   newTarget.CharacterStats.ApplyStatus(StatusType.Strength, 2);
                   break;

               case 3:
                   int blockValue = Mathf.RoundToInt(
                       actionParameters.Value +
                       GameManager.PersistentGameplayData.proficiency +
                       actionParameters.SelfCharacter.CharacterStats
                           .StatusDict[StatusType.Fortitude].StatusValue
                   );

                   newTarget.CharacterStats.ApplyStatus(StatusType.Block, blockValue);
                   break;
           }



            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.StartingGear);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}