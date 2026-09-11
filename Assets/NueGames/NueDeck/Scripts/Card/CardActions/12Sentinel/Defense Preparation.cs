using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class DefensePreparation : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.DefensePreparation;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (!selfCharacter) return;

            
            selfCharacter.CharacterStats.ApplyStatus(
                StatusType.Vigilance,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + selfCharacter.CharacterStats.StatusDict[StatusType.Fortitude].StatusValue));
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Fortitude, 1);

            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.DefensePreparation);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}