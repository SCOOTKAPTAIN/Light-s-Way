using System;
using System.Collections.Generic;
using UnityEngine;

public enum EncounterEventKind
{
    None,
    RestSite,
    NormalEvent,
    OminousEvent
}

public static class EncounterEventContext
{
    private const int RepeatEventCooldownLength = 2;
    private static readonly HashSet<string> ConsumedOneTimeEventIds = new(StringComparer.Ordinal);
    private static readonly Queue<string> RecentRepeatEventIds = new();

    public static bool IsRestSite { get; private set; }
    public static bool IsNormalEvent { get; private set; }
    public static bool IsOminousEvent { get; private set; }
    public static bool UseOneTimePool { get; private set; }
    public static float OminousChance { get; private set; }

    public static void SelectRestSite()
    {
        ResetSelection();
        IsRestSite = true;
    }

    public static void SelectRandomEvent(int light)
    {
        ResetSelection();

        float normalizedLight = Mathf.Clamp01(light / 100f);
        OminousChance = Mathf.Lerp(0.5f, 0f, normalizedLight);
        IsOminousEvent = UnityEngine.Random.value < OminousChance;
        IsNormalEvent = !IsOminousEvent;
        UseOneTimePool = UnityEngine.Random.value < 0.5f;
    }

    public static bool HasConsumedOneTimeEvent(string eventId)
    {
        return !string.IsNullOrWhiteSpace(eventId) && ConsumedOneTimeEventIds.Contains(eventId);
    }

    public static void ConsumeOneTimeEvent(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
            ConsumedOneTimeEventIds.Add(eventId);
    }

    public static bool IsRepeatEventOnCooldown(string eventId)
    {
        return !string.IsNullOrWhiteSpace(eventId) && RecentRepeatEventIds.Contains(eventId);
    }

    public static void RecordRepeatEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        RecentRepeatEventIds.Enqueue(eventId);
        while (RecentRepeatEventIds.Count > RepeatEventCooldownLength)
            RecentRepeatEventIds.Dequeue();
    }

    public static void ResetRun()
    {
        ConsumedOneTimeEventIds.Clear();
        RecentRepeatEventIds.Clear();
        ResetSelection();
    }

    public static void ResetSelectionForSceneExit()
    {
        ResetSelection();
    }

    private static void ResetSelection()
    {
        IsRestSite = false;
        IsNormalEvent = false;
        IsOminousEvent = false;
        UseOneTimePool = false;
        OminousChance = 0f;
    }
}