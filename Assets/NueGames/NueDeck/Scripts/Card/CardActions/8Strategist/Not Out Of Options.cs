using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class NotOutofOptions : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.NotOutOfOptions;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var anchor = CombatManager.Instance.EnemiesFxAnchor;
            if (CollectionManager != null)
                CollectionManager.DrawCards(Mathf.RoundToInt(actionParameters.Value));
            else
                Debug.LogError("There is no CollectionManager");
            
            if (FxManager != null)
                 FxManager.Instance.PlayFxAtPosition(anchor.position, FxType.NotOutOfOptions, 0.3f);  
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}