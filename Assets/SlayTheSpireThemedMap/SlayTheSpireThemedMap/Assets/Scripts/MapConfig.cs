using System.Collections.Generic;
using OneLine;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu]
    public class MapConfig : ScriptableObject
    {
        public List<NodeBlueprint> nodeBlueprints;
        [Tooltip("Nodes that will be used on layers with Randomize Nodes > 0")]
        public List<NodeType> randomNodes = new List<NodeType>
            {NodeType.Mystery, NodeType.Store, NodeType.Treasure, NodeType.MinorEnemy, NodeType.RestSite};
        [Tooltip("Weighted random node rules. If empty, randomNodes is used with weight 1 and no cooldown.")]
        public List<RandomNodeRule> randomNodeRules = new List<RandomNodeRule>();
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        [OneLineWithHeader]
        public IntMinMax numOfPreBossNodes;
        [OneLineWithHeader]
        public IntMinMax numOfStartingNodes;

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;
        public List<MapLayer> layers;
    }

    [System.Serializable]
    public class RandomNodeRule
    {
        public NodeType nodeType;
        [Min(0f)] public float weight = 1f;
        [Min(0)] public int cooldown;
    }
}