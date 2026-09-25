using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.UI;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SacrificeKnowledge : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SacrificeKnowledge;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!(actionParameters.TargetCharacter is EnemyBase enemy))
                return;

            if (enemy.CharacterStats.StatusDict[StatusType.TotalAssimilation].IsActive)
            {
                FxManager?.SpawnStaticText(enemy.transform, "The Hexer is not interested in your offering...", 0, 1);
                return;
            }

            var panel = Object.FindFirstObjectByType<SacrificeKnowledgePanel>(FindObjectsInactive.Include);
            if (panel == null)
            {
                Debug.LogWarning("Sacrifice Knowledge requires a SacrificeKnowledgePanel in the combat UI.");
                return;
            }

            panel.OpenForTarget(enemy);
        }
    }
}
