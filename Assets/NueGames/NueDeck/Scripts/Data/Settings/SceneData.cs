﻿using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Settings
{
    [CreateAssetMenu(fileName = "Scene Data", menuName = "NueDeck/Settings/Scene", order = 2)]
    public class SceneData : ScriptableObject
    {
        public int mainMenuSceneIndex = 0;
        public int mapSceneIndex = 1;
        public int combatSceneIndex = 2;

        public int dialogueSceneIndex = 4;
        public int merchantSceneIndex = 5;
        public int endingSceneIndex = 10;
    }
}