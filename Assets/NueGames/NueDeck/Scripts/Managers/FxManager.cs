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
         [SerializeField] private FloatingText floatingTextPrefabYellow;
        [SerializeField] private FloatingText floatingTextPrefabGrey;
        [SerializeField] private FloatingText floatingTextPrefabOrange;

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
            // Damage numbers get a larger spread so multiple hits don't perfectly overlap
            var jitter = new Vector3(
                Random.Range(-0.35f, 0.35f),
                Random.Range(0f, 0.25f),
                Random.Range(-0.05f, 0.05f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabRed, spawnPos, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnStaticText(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Static texts get a small jitter to avoid perfect stacking
            var jitter = new Vector3(
                Random.Range(-0.12f, 0.12f),
                Random.Range(0f, 0.1f),
                Random.Range(-0.02f, 0.02f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabRed, spawnPos, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextGreen(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Healing texts are softer and less spread than damage texts
            var jitter = new Vector3(
                Random.Range(-0.22f, 0.22f),
                Random.Range(0f, 0.16f),
                Random.Range(-0.03f, 0.03f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabGreen, spawnPos, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextBlue(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Block/guard texts should be fairly static and clustered, small jitter only
            var jitter = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(0f, 0.08f),
                Random.Range(-0.02f, 0.02f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabBlue, spawnPos, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextYellow(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Block/guard texts should be fairly static and clustered, small jitter only
            var jitter = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(0f, 0.08f),
                Random.Range(-0.02f, 0.02f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabYellow, spawnPos, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextGrey(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Grey text for fully blocked damage
            var jitter = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(0f, 0.08f),
                Random.Range(-0.02f, 0.02f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabGrey, spawnPos, Quaternion.identity);
            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnFloatingTextOrange(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            // Grey text for fully blocked damage
            var jitter = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(0f, 0.08f),
                Random.Range(-0.02f, 0.02f)
            );
            var spawnPos = targetTransform.position + jitter;
            var cloneText = Instantiate(floatingTextPrefabOrange, spawnPos, Quaternion.identity);
            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 2 : -2;
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
            var clone = Instantiate(prefab, targetTransform.position + offset, Quaternion.identity);
            try
            {
                clone.transform.SetParent(targetTransform, true);
            }
            catch (Exception)
            {
                // Parent may fail for some prefab types; ignore to preserve instantiation
            }
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
                // Debug important FX spawns for troubleshooting
                if (targetFx == FxType.Frozen || targetFx == FxType.Combustion || targetFx == FxType.FrozenMirror2 || targetFx == FxType.BlazingSurge2)
                {
                    Debug.Log($"[FxManager] Spawning {targetFx} at {position + offset}");
                }
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