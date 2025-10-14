using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SwordandShield2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SwordandShield2;
        public override void DoAction(CardActionParameters actionParameters)
        {
                var selfCharacter = actionParameters.SelfCharacter;
        

            selfCharacter.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));


            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.SwordandShield2);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.SwordandShield2);

                
              
        }
    }
}