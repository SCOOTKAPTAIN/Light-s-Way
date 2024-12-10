using System;
using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class DialogueAudioManager : MonoBehaviour
{
    public static DialogueAudioManager instance;
    public Sound[] music_sound, sfx_sound;
    public AudioSource music_source, sfx_source;
    private void Awake()
    {
        if(instance == null) // to make things easier (easier to access)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //PlayMusic("Theme"); // When game start play theme
    }

    public void BossMusic()
    {
        if(GameManager.Instance.PersistentGameplayData.ActNumber == 11)
        {
            PlayMusic("FinalBoss");
            
        }else
        {
            PlayMusic("ChapterBoss");
        }
    }

    public void DynamicMusic(string type)
    {
        if(type == "battle")
        {
            switch(GameManager.Instance.PersistentGameplayData.light)
            {
                case >= 90 and <= 100:
                PlayMusic("100LightBattle");
                break;

                case >= 50 and <= 89:
                PlayMusic("89LightBattle");                
                break;

                case >= 25 and <= 49:
                PlayMusic("49LightBattle");
                break;

                case >= 10 and <= 24:
                PlayMusic("24LightBattle");
                break;

                case >= 1 and <= 9:
                PlayMusic("9LightBattle");               
                break;

                case 0:
                PlayMusic("0LightBattle");
                
                break;
            }

        }
        else if(type == "map")
        {
            switch(GameManager.Instance.PersistentGameplayData.light)
            {
                case >= 90 and <= 100:
                PlayMusic("100LightMap");
                break;
                
                case >= 50 and <= 89:
                PlayMusic("89LightMap");                
                break;

                case >= 25 and <= 49:
                PlayMusic("49LightMap");
                break;

                case >= 10 and <= 24:
                PlayMusic("24LightMap");
                break;

                case >= 1 and <= 9:
                PlayMusic("9LightMap");               
                break;

                case 0:
                PlayMusic("0LightMap");
                
                break;
            }

        }

    }

    public void PlayMusic(string name) // Call this function to play music
{
    // Search for the audio in the array
    Sound sound = Array.Find(music_sound, x => x.name == name); 
    if (sound != null)
    {
        // Check if the music source is already playing the same clip
        if (music_source.isPlaying && music_source.clip == sound.clip)
        {
            Debug.Log("Music is already playing.");
            return; // Do not restart the same clip
        }

        // Otherwise, play the new clip
        music_source.clip = sound.clip;
        music_source.Play();
    }
    else
    {
        Debug.Log("Music Not Found");
    }
}

    // public void PlayMusic(string name) //Call this function from any script u want to add music
    // {
    //     Sound sound = Array.Find(music_sound, x => x.name == name); //Search audio from array
    //     if (sound != null)
    //     {
    //         music_source.clip = sound.clip;
    //         music_source.Play();
    //     }
    //     else
    //     {
    //         Debug.Log("Music Not Found");
    //     }
    // }
    public void PlaySFX(string name) //Call this function from any script u want to add SFX
    {
        Sound sound = Array.Find(sfx_sound, x => x.name == name); //Search audio from array
        if (sound != null)
        {
            sfx_source.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.Log("SFX Not Found");
        }
    }
    public void ToggleMusic()
    {
        music_source.mute = !music_source.mute;
    }
    public void ToggleSFX()
    {
        sfx_source.mute = !sfx_source.mute;
    }
    public void MusicVolume(float volume)
    {
        music_source.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfx_source.volume = volume;
    }
    public void ChangeMusic(string name)
    {
        Sound sound = Array.Find(music_sound, x => x.name == name);
        if (sound != null)
        {
            if(music_source.clip == null || music_source.clip == sound.clip)
            {
                Debug.Log("Same Song Played");
                return;
            }
            music_source.Stop(); 
            music_source.clip = sound.clip;
            music_source.clip.LoadAudioData();
            music_source.Play(); 
        }
        else
        {
            Debug.Log("Music Not Found");
        }
    }
}
