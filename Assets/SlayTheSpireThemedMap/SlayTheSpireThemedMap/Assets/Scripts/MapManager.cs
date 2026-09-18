using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        [System.Serializable]
        private class ActMapConfig
        {
            public int actNumber;
            public MapConfig mapConfig;
        }

        public MapConfig config;
        public MapView view;
        [SerializeField] private List<ActMapConfig> actMapConfigs = new List<ActMapConfig>();

        public Map CurrentMap { get; private set; }

        private string MapSaveKey => $"Map_Act_{GetCurrentAct()}";

        private void Start()
        {
            ApplyMapConfigForCurrentAct();

            if (PlayerPrefs.HasKey(MapSaveKey))
            {
                string mapJson = PlayerPrefs.GetString(MapSaveKey);
                Map map = JsonConvert.DeserializeObject<Map>(mapJson);
                // using this instead of .Contains()
                if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
                {
                    // payer has already reached the boss, generate a new map
                    GenerateNewMap();
                }
                else
                {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            }
            else
            {
                GenerateNewMap();
            }
           // GenerateNewMap();
        }

        public void GenerateNewMap()
        {
            ApplyMapConfigForCurrentAct();
            Map map = MapGenerator.GetMap(config);
            CurrentMap = map;
           // Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public void SaveMap()
        {
            if (CurrentMap == null) return;

            string json = JsonConvert.SerializeObject(CurrentMap, Formatting.Indented,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            PlayerPrefs.SetString(MapSaveKey, json);
            PlayerPrefs.Save();
        }

        private int GetCurrentAct()
        {
            return GameManager.Instance != null && GameManager.Instance.PersistentGameplayData != null
                ? GameManager.Instance.PersistentGameplayData.ActNumber
                : 0;
        }

        private void ApplyMapConfigForCurrentAct()
        {
            var actConfig = actMapConfigs.Find(entry => entry != null && entry.actNumber == GetCurrentAct());
            if (actConfig != null && actConfig.mapConfig != null)
                config = actConfig.mapConfig;

            if (config == null)
            {
                Debug.LogError($"No MapConfig is assigned for Act {GetCurrentAct()}.");
                return;
            }

            if (view != null)
            {
                if (view.allMapConfigs == null)
                    view.allMapConfigs = new List<MapConfig>();

                if (!view.allMapConfigs.Contains(config))
                    view.allMapConfigs.Add(config);
            }
        }

        private void OnApplicationQuit()
        {
            SaveMap();
        }
    }
}
