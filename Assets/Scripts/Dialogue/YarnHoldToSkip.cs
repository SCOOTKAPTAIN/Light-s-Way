using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public sealed class YarnHoldToSkip : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private bool includeRightControl = true;
    [SerializeField] private float advanceInterval = 0.08f;

    private float nextAdvanceTime;

    private void Update()
    {
        if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool controlHeld = keyboard.leftCtrlKey.isPressed
            || includeRightControl && keyboard.rightCtrlKey.isPressed;

        if (!controlHeld)
        {
            nextAdvanceTime = 0f;
            return;
        }

        if (Time.unscaledTime < nextAdvanceTime)
            return;

        dialogueRunner.RequestNextLine();
        nextAdvanceTime = Time.unscaledTime + Mathf.Max(advanceInterval, 0.02f);
    }
}