using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NueGames.NueDeck.Scripts.Utils;
using NueGames.NueDeck.Scripts.Managers;


public class MainMenu : MonoBehaviour
{

    [SerializeField] private Animator FlashScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button Options;
    [SerializeField] private Button Quit;
    [SerializeField] private AudioSource audioSource; 

     private void Start() 
     {
        GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth = 100;
        PlayerPrefs.DeleteKey("Map");
        
    }
    public void PlayGame()
    {
        playButton.interactable = false;
        Options.interactable = false;
        Quit.interactable = false;
        StartCoroutine(PlayPress());
        EventSystem.current.SetSelectedGameObject(null);

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator PlayPress()
   {
    DialogueAudioManager.instance.music_source.Stop();
    FlashScreen.Play("White");
    yield return new WaitForSeconds(3);
    FlashScreen.Play("Black");
    yield return new WaitForSeconds(4);
    SceneManager.LoadScene("Intro");
        

    }

    
}
