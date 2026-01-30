using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class IllusionofVictory3 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.IllusionOfVictory3;
        public override void DoAction(CardActionParameters actionParameters)
        {
           var targetCharacter = actionParameters.TargetCharacter;
           var selfCharacter = actionParameters.SelfCharacter;
           var anchor = CombatManager.Instance.EnemiesFxAnchor;

            if (!targetCharacter) return;

            // Check if target has Block before reducing it
            if (targetCharacter.CharacterStats.StatusDict.ContainsKey(StatusType.Block))
            {
                var currentBlock = targetCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue;
                var blockReduction = Mathf.RoundToInt(currentBlock * 0.25f);
                
                if (blockReduction > 0)
                {
                    targetCharacter.CharacterStats.ApplyStatus(StatusType.Block, -blockReduction);
                }
            }
            

            if (FxManager != null)
                 FxManager.PlayFx(anchor.transform, FxType.IllusionOfVictory3, new Vector3(0f, 0.4f, 0f));             
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.IllusionOfVictory3);
        }
    }
}