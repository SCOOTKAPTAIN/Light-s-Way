using System;
using System.Collections.Generic;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Tutorial
{
    public enum TutorialCompletionPolicy
    {
        OncePerRun,
        CompleteForever
    }

    [CreateAssetMenu(fileName = "Tutorial", menuName = "Light's Way/Tutorial")]
    public sealed class TutorialDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class Page
        {
            [TextArea(2, 5)]
            public string title;

            [TextArea(4, 12)]
            public string body;

            public Sprite image;
        }

        [SerializeField] private string tutorialId;
        [SerializeField] private TutorialCompletionPolicy completionPolicy = TutorialCompletionPolicy.OncePerRun;
        [SerializeField] private List<Page> pages = new List<Page>();

        public string TutorialId => string.IsNullOrWhiteSpace(tutorialId) ? name : tutorialId;
        public TutorialCompletionPolicy CompletionPolicy => completionPolicy;
        public IReadOnlyList<Page> Pages => pages;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(tutorialId))
                tutorialId = name;
        }
    }
}
