using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Settings
{
    [CreateAssetMenu(fileName = "Scene Data", menuName = "NueDeck/Settings/Scene", order = 2)]
    public class SceneData : ScriptableObject
    {
        public int mainMenuSceneIndex = 0;
        public int mapSceneIndex = 5;
        public int combatSceneIndex = 3;
        public int dialogueSceneIndex = 7;
        public int merchantSceneIndex = 6;
        public int endingSceneIndex = 10;
    }
}