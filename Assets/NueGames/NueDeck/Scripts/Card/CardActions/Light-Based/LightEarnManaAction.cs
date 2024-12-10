using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LightEarnManaAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LightManaDraw;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (CombatManager != null){
                GameManager.PersistentGameplayData.ChangeLight(-10);
                CombatManager.RefillMana();
            }else
                Debug.LogError("There is no CombatManager");

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.Buff);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}