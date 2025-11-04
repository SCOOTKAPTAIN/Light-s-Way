using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private AudioManager(){}
        public static AudioManager Instance { get; private set; }

        [SerializeField]private AudioSource musicSource;
        [SerializeField]private AudioSource sfxSource;
        [SerializeField]private AudioSource buttonSource;
        
        [SerializeField] private List<SoundProfileData> soundProfileDataList;
        
        private Dictionary<AudioActionType, SoundProfileData> _audioDict = new Dictionary<AudioActionType, SoundProfileData>();
    // Tracks last played time per audio action to allow debounced playback and avoid overlapping noise
    private readonly Dictionary<AudioActionType, float> _lastPlayedTime = new Dictionary<AudioActionType, float>();
        
        #region Setup
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                for (int i = 0; i < Enum.GetValues(typeof(AudioActionType)).Length; i++)
                    _audioDict.Add((AudioActionType)i,soundProfileDataList.FirstOrDefault(x=>x.AudioType == (AudioActionType)i));
            }
        }

        #endregion
        
        #region Public Methods

        public void PlayMusic(AudioClip clip)
        {
            if (!clip) return;
            
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayMusic(AudioActionType type)
        {
            var clip = _audioDict[type].GetRandomClip();
            if (clip)
                PlayMusic(clip);
        }

        public void PlayOneShot(AudioActionType type)
        {
            var clip = _audioDict[type].GetRandomClip();
            if (clip)
                PlayOneShot(clip);
        }
        
        public void PlayOneShotButton(AudioActionType type)
        {
            var clip = _audioDict[type].GetRandomClip();
            if (clip)
                PlayOneShotButton(clip);
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip)
                sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Play a one-shot SFX for the given action type but skip playing if the same action was played
        /// more recently than minInterval seconds. Useful to prevent overlapping rapid sounds (e.g., bleed ticks).
        /// </summary>
        public void PlayOneShotDebounced(AudioActionType type, float minInterval = 0.25f)
        {
            var clip = _audioDict[type]?.GetRandomClip();
            if (clip == null) return;

            var now = Time.time;
            if (!_lastPlayedTime.TryGetValue(type, out var last)) last = -999f;
            if (now - last < minInterval) return;

            _lastPlayedTime[type] = now;
            PlayOneShot(clip);
        }
        
        public void PlayOneShotButton(AudioClip clip)
        {
            if (clip)
                buttonSource.PlayOneShot(clip);
        }

        #endregion
    }
}
