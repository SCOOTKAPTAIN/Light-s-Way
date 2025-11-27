using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NueGames.NueDeck.Scripts.Managers
{
    public class FxManager : MonoBehaviour
    {
        public FxManager(){}
        public static FxManager Instance { get; private set; }
    
        [Header("References")] 
        [SerializeField] private List<FxBundle> fxList;

        [Header("Floating Text")]
        [SerializeField] private FloatingText floatingTextPrefabRed;
        [SerializeField] private FloatingText floatingTextPrefabGreen;
        [SerializeField] private FloatingText floatingTextPrefabBlue;

        public Dictionary<FxType, GameObject> FXDict { get; private set;}= new Dictionary<FxType, GameObject>();
        public List<FxBundle> FXList => fxList;
        
        // Track spawned FX to prevent duplicates in the same frame
        private Dictionary<string, int> _spawnedThisFrame = new Dictionary<string, int>();
        private int _currentFrame = -1;

        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);
                for (int i = 0; i < Enum.GetValues(typeof(FxType)).Length; i++)
                    FXDict.Add((FxType)i,FXList.FirstOrDefault(x=>x.FxType == (FxType)i)?.FxPrefab);
            }
        }
        #endregion

        #region Public Methods

        public void SpawnFloatingText(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            var cloneText = Instantiate(floatingTextPrefabRed, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnStaticText(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            var cloneText = Instantiate(floatingTextPrefabRed, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextGreen(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            var cloneText = Instantiate(floatingTextPrefabGreen, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextBlue(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            var cloneText = Instantiate(floatingTextPrefabBlue, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }


        public void PlayFx(Transform targetTransform, FxType targetFx)
        {
            // Default behaviour: spawn slightly above target
            PlayFx(targetTransform, targetFx, Vector3.up * 0.2f);
        }

        /// <summary>
        /// Play an FX at a transform with a positional offset (local world offset).
        /// </summary>
        public void PlayFx(Transform targetTransform, FxType targetFx, Vector3 offset)
        {
            if (!FXDict.TryGetValue(targetFx, out var prefab) || prefab == null) return;
            Instantiate(prefab, targetTransform.position + offset, Quaternion.identity);
        }

        /// <summary>
        /// Play an FX at an arbitrary world position. Useful for group effects (all enemies/all allies).
        /// </summary>
        public void PlayFxAtPosition(Vector3 position, FxType targetFx, float yOffset = 0.2f)
        {
            // Backwards-compatible float overload (y offset only)
            PlayFxAtPosition(position, targetFx, new Vector3(0f, yOffset, 0f));
        }

        /// <summary>
        /// Play an FX at an arbitrary world position with a full XYZ offset.
        /// Only spawns once per frame for the same FxType to prevent duplicates.
        /// </summary>
        public void PlayFxAtPosition(Vector3 position, FxType targetFx, Vector3 offset)
        {
            if (!FXDict.TryGetValue(targetFx, out var prefab) || prefab == null) return;
            
            // Reset tracking if we're in a new frame
            if (_currentFrame != Time.frameCount)
            {
                _currentFrame = Time.frameCount;
                _spawnedThisFrame.Clear();
            }
            
            // Create a unique key for this FX type and position
            var key = $"{targetFx}_{position.x:F1}_{position.y:F1}_{position.z:F1}";
            
            // Only spawn if we haven't already spawned this FX at this position this frame
            if (!_spawnedThisFrame.ContainsKey(key))
            {
                _spawnedThisFrame[key] = 1;
                Instantiate(prefab, position + offset, Quaternion.identity);
            }
        }
        #endregion
        
    }

    [Serializable]
    public class FxBundle
    {
        [SerializeField] private FxType fxType;
        [SerializeField] private GameObject fxPrefab;
        public FxType FxType => fxType;
        public GameObject FxPrefab => fxPrefab;
    }
}