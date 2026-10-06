using System;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using Yarn.Unity;

public sealed class YarnIntroDialogue : MonoBehaviour
{
    [Header("Yarn")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode = "intro";
    [SerializeField] private float startDelay = 1f;

    [Header("Intro Presentation")]
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private string backgroundState = "Intro_1";
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private string fadeOutState = "FadeOut";
    [SerializeField] private float fadeOutDelay = 1f;

    private async void Start()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("YarnIntroDialogue needs a DialogueRunner reference.", this);
            return;
        }

        if (backgroundAnimator != null && !string.IsNullOrWhiteSpace(backgroundState))
        {
            backgroundAnimator.Play(backgroundState);
        }

        if (startDelay > 0f)
        {
            await YarnTask.Delay(TimeSpan.FromSeconds(startDelay));
        }

        await dialogueRunner.StartDialogue(startNode);

        if (fadeAnimator != null && !string.IsNullOrWhiteSpace(fadeOutState))
        {
            fadeAnimator.Play(fadeOutState);

            if (fadeOutDelay > 0f)
            {
                await YarnTask.Delay(TimeSpan.FromSeconds(fadeOutDelay));
            }
        }

        var uiManager = UIManager.Instance;
        var gameManager = GameManager.Instance;
        if (uiManager == null || gameManager == null)
        {
            Debug.LogError("Cannot transition from the intro because GameManager or UIManager is missing.", this);
            return;
        }

        uiManager.SetCanvas(uiManager.CombatCanvas, false, true);
        uiManager.SetCanvas(uiManager.InformationCanvas, true, false);
        uiManager.SetCanvas(uiManager.RewardCanvas, false, true);
        uiManager.ChangeScene(gameManager.SceneData.mapSceneIndex);
    }
}