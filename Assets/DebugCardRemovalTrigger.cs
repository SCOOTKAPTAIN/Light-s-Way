using UnityEngine;

public class DebugCardRemovalTrigger : MonoBehaviour
{
    public void TriggerCardRemoval()
    {
        Object.FindFirstObjectByType<CardRemovalManager>()?.OpenCardRemovalScreen();

    }
}
