using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter Event Pool", menuName = "Light's Way/Encounter Event Pool")]
public sealed class EncounterEventPool : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        [SerializeField] private string eventId;
        [SerializeField] private string yarnNode;
        [SerializeField, Min(1)] private int weight = 1;

        public string EventId => eventId;
        public string YarnNode => yarnNode;
        public int Weight => Mathf.Max(1, weight);
    }

    [SerializeField] private List<Entry> events = new List<Entry>();

    public IReadOnlyList<Entry> Events => events;
}