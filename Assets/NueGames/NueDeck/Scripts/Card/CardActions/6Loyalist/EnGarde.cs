using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class EnGarde : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.EnGarde;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

           newTarget.CharacterStats.ApplyStatus(
               StatusType.Armor,
               Mathf.RoundToInt(CardScaling.AddAffinity(1f)));

           newTarget.CharacterStats.ApplyStatus(
               StatusType.Pursuit,
               Mathf.RoundToInt(CardScaling.AddAffinity(8f)));

            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.EnGarde);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}