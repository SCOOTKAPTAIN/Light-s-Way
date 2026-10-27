using System.Collections.Generic;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Tutorial
{
    [DefaultExecutionOrder(-20)]
    public sealed class TutorialManager : MonoBehaviour
    {
        private const string PermanentKeyPrefix = "Light's Way.Tutorial.Completed.";

        public static TutorialManager Instance { get; private set; }

        [SerializeField] private TutorialPanel tutorialPanel;

        private readonly HashSet<string> shownThisRun = new HashSet<string>();
        private TutorialDefinition activeTutorial;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (tutorialPanel == null)
                tutorialPanel = GetComponentInChildren<TutorialPanel>(true);
        }

        public bool TryShow(TutorialDefinition definition)
        {
            if (definition == null || definition.Pages == null || definition.Pages.Count == 0)
            {
                Debug.LogError("Tutorial request ignored because the definition has no pages.", this);
                return false;
            }

            if (activeTutorial != null)
                return false;

            var id = definition.TutorialId;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"Tutorial '{definition.name}' has no valid identifier.", definition);
                return false;
            }

            if (definition.CompletionPolicy == TutorialCompletionPolicy.CompleteForever &&
                PlayerPrefs.GetInt(PermanentKeyPrefix + id, 0) == 1)
                return false;

            if (shownThisRun.Contains(id))
                return false;

            if (tutorialPanel == null)
                tutorialPanel = FindFirstObjectByType<TutorialPanel>(FindObjectsInactive.Include);

            if (tutorialPanel == null)
            {
                Debug.LogError(
                    $"Tutorial '{id}' could not open because no TutorialPanel is configured or present in the scene.",
                    this);
                return false;
            }

            activeTutorial = definition;
            tutorialPanel.Open(definition, OnTutorialClosed);
            return true;
        }

        public void ResetRunTutorials()
        {
            shownThisRun.Clear();
        }

        private void OnTutorialClosed(TutorialDefinition definition, bool completed)
        {
            if (definition == null || activeTutorial != definition)
                return;

            if (completed)
            {
                shownThisRun.Add(definition.TutorialId);

                if (definition.CompletionPolicy == TutorialCompletionPolicy.CompleteForever)
                {
                    PlayerPrefs.SetInt(PermanentKeyPrefix + definition.TutorialId, 1);
                    PlayerPrefs.Save();
                }
            }

            activeTutorial = null;
        }
    }
}
