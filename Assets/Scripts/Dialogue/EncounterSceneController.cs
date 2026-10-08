using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public sealed class EncounterSceneController : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;

    [Header("Event Pools")]
    [SerializeField] private EncounterEventPool restSitePool;
    [SerializeField] private EncounterEventPool normalRepeatPool;
    [SerializeField] private EncounterEventPool normalOneTimePool;
    [SerializeField] private EncounterEventPool ominousRepeatPool;
    [SerializeField] private EncounterEventPool ominousOneTimePool;

    private bool hasStarted;
    private bool selectedOneTimePool;
    private bool selectedRepeatPool;

    private async void Start()
    {
        if (hasStarted)
            return;

        hasStarted = true;
        await PlayPendingEvent();
    }

    private async YarnTask PlayPendingEvent()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner == null)
        {
            Debug.LogError("Cannot start encounter dialogue because DialogueRunner is missing.", this);
            return;
        }

        EncounterEventPool pool = SelectPool();
        EncounterEventPool.Entry entry = SelectEntry(pool);
        if (entry == null || string.IsNullOrWhiteSpace(entry.YarnNode))
        {
            Debug.LogError("No eligible encounter dialogue event is configured for the selected pool.", this);
            return;
        }

        if (selectedOneTimePool)
            EncounterEventContext.ConsumeOneTimeEvent(GetEventId(entry));
        else if (selectedRepeatPool)
            EncounterEventContext.RecordRepeatEvent(GetEventId(entry));

        await dialogueRunner.StartDialogue(entry.YarnNode);

        EncounterEventContext.ResetSelectionForSceneExit();
    }

    private EncounterEventPool SelectPool()
    {
        selectedOneTimePool = false;
        selectedRepeatPool = false;

        if (EncounterEventContext.IsRestSite)
            return restSitePool;

        if (EncounterEventContext.IsOminousEvent)
        {
            if (EncounterEventContext.UseOneTimePool && HasAvailableEntries(ominousOneTimePool))
            {
                selectedOneTimePool = true;
                return ominousOneTimePool;
            }

            selectedRepeatPool = true;
            return ominousRepeatPool;
        }

        if (EncounterEventContext.UseOneTimePool && HasAvailableEntries(normalOneTimePool))
        {
            selectedOneTimePool = true;
            return normalOneTimePool;
        }

        selectedRepeatPool = true;
        return normalRepeatPool;
    }

    private EncounterEventPool.Entry SelectEntry(EncounterEventPool pool)
    {
        if (pool == null || pool.Events == null)
            return null;

        var candidates = new List<EncounterEventPool.Entry>();
        foreach (EncounterEventPool.Entry entry in pool.Events)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.YarnNode))
                continue;

            if (entry.Weight <= 0)
                continue;

            if (selectedOneTimePool && EncounterEventContext.HasConsumedOneTimeEvent(GetEventId(entry)))
                continue;

            candidates.Add(entry);
        }

        if (candidates.Count == 0)
            return null;

        if (selectedRepeatPool)
        {
            var availableCandidates = candidates.FindAll(candidate =>
                !EncounterEventContext.IsRepeatEventOnCooldown(GetEventId(candidate)));
            if (availableCandidates.Count > 0)
                candidates = availableCandidates;
        }

        int totalWeight = 0;
        foreach (EncounterEventPool.Entry candidate in candidates)
            totalWeight += candidate.Weight;

        int roll = Random.Range(0, totalWeight);
        foreach (EncounterEventPool.Entry candidate in candidates)
        {
            roll -= candidate.Weight;
            if (roll < 0)
                return candidate;
        }

        return candidates[candidates.Count - 1];
    }

    private bool HasAvailableEntries(EncounterEventPool pool)
    {
        if (pool == null || pool.Events == null)
            return false;

        foreach (EncounterEventPool.Entry entry in pool.Events)
        {
            if (entry != null && entry.Weight > 0 && !string.IsNullOrWhiteSpace(entry.YarnNode) &&
                !EncounterEventContext.HasConsumedOneTimeEvent(GetEventId(entry)))
                return true;
        }

        return false;
    }

    private static string GetEventId(EncounterEventPool.Entry entry)
    {
        return string.IsNullOrWhiteSpace(entry.EventId) ? entry.YarnNode : entry.EventId;
    }
}