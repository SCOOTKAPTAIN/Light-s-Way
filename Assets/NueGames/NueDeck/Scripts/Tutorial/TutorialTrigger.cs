using UnityEngine;

namespace NueGames.NueDeck.Scripts.Tutorial
{
    public sealed class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialDefinition tutorial;
        [SerializeField] private bool showOnStart;

        private void Start()
        {
            if (showOnStart)
                TryShow();
        }

        public void TryShow()
        {
            if (tutorial == null)
            {
                Debug.LogError($"TutorialTrigger on '{name}' has no tutorial definition.", this);
                return;
            }

            if (TutorialManager.Instance == null)
            {
                Debug.LogError("TutorialTrigger could not find a TutorialManager in the scene.", this);
                return;
            }

            TutorialManager.Instance.TryShow(tutorial);
        }
    }
}
