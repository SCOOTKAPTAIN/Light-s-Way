﻿using System;
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
        [SerializeField] private FloatingText floatingTextPrefab;
        

        public Dictionary<FxType, GameObject> FXDict { get; private set; }= new Dictionary<FxType, GameObject>();
        public List<FxBundle> FXList => fxList;

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

        public void SpawnFloatingText(Transform targetTransform, string text, int xDir = 0, int yDir = -1)
        {
            var cloneText = Instantiate(floatingTextPrefab, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }

        public void SpawnStaticText(Transform targetTransform, string text, int xDir = 0, int yDir = 1)
        {
            var cloneText = Instantiate(floatingTextPrefab, targetTransform.position, Quaternion.identity);

            if (xDir == 0)
                xDir = Random.value >= 0.5f ? 1 : -1;
            cloneText.PlayText(text, xDir, yDir);
        }


        public void PlayFx(Transform targetTransform, FxType targetFx)
        {
            // Instantiate(FXDict[targetFx], targetTransform);
           Instantiate(FXDict[targetFx], targetTransform.position + Vector3.up * 0.2f, Quaternion.identity);
        }

        /// <summary>
        /// Play an FX at an arbitrary world position. Useful for group effects (all enemies/all allies).
        /// </summary>
        public void PlayFxAtPosition(Vector3 position, FxType targetFx, float yOffset = 0.2f)
        {
            if (!FXDict.TryGetValue(targetFx, out var prefab) || prefab == null) return;
            Instantiate(prefab, position + Vector3.up * yOffset, Quaternion.identity);
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