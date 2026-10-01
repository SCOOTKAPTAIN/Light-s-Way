using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

public sealed class YarnTypingSoundView : ActionMarkupHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingClip;
    [SerializeField] private int playEveryCharacters = 2;
    [SerializeField] private float minimumInterval = 0.035f;

    private float lastPlayedTime = float.NegativeInfinity;
    private int characterCount;

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        characterCount = 0;
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
    }

    public override void OnLineDisplayComplete()
    {
    }

    public override void OnLineWillDismiss()
    {
    }

    public override YarnTask OnCharacterWillAppear(
        int currentCharacterIndex,
        MarkupParseResult line,
        CancellationToken cancellationToken)
    {
        characterCount++;

        if (typingClip == null || audioSource == null)
            return YarnTask.CompletedTask;

        bool isCharacterInterval = playEveryCharacters <= 1
            || characterCount % playEveryCharacters == 0;
        bool isTimeInterval = Time.unscaledTime - lastPlayedTime >= minimumInterval;

        if (isCharacterInterval && isTimeInterval)
        {
            audioSource.PlayOneShot(typingClip);
            lastPlayedTime = Time.unscaledTime;
        }

        return YarnTask.CompletedTask;
    }
}