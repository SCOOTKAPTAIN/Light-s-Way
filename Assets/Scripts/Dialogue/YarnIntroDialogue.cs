using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public sealed class YarnIntroDialogue : MonoBehaviour
{
    [Header("Yarn")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode = "Start";
    [SerializeField] private float startDelay = 1f;

    [Header("Intro Presentation")]
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private string backgroundState = "Intro_1";
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private string fadeOutState = "FadeOut";
    [SerializeField] private float fadeOutDelay = 1f;
    [SerializeField] private string nextSceneName;

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

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("YarnIntroDialogue finished without a next scene name.", this);
            return;
        }

        if (fadeAnimator != null && !string.IsNullOrWhiteSpace(fadeOutState))
        {
            fadeAnimator.Play(fadeOutState);

            if (fadeOutDelay > 0f)
            {
                await YarnTask.Delay(TimeSpan.FromSeconds(fadeOutDelay));
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }
}