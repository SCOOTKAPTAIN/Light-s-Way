using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
//DialogueManager.GetInstance().EnterDialogueMode(scene1, backgroundAnimator);
//StartCoroutine(Flow());
//yield return new WaitForSeconds(5);
//FadeBox.Play("FadeOut");
//FadeBox.Play("FadeIn");
public class IntroDialogue : MonoBehaviour
{

    [Header("Background Animator")]
    [SerializeField] private Animator backgroundAnimator;

    [SerializeField] private Animator effectAnimator;

    [SerializeField] private Animator FadeBox;

    [SerializeField] private TextAsset scene1;

    private bool GotoGame;
    [SerializeField] private string scenename;
    

    private void Start()
    {
        backgroundAnimator.Play("Intro_1");
        DialogueAudioManager.instance.PlayMusic("clock");
        StartCoroutine(Flow());
    }


    private void Update()
    {
        if(GotoGame == true && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
           // IntroConfirm();
            StartCoroutine(GameGo());
        }

    }

    public void IntroConfirm()
    {
        Debug.Log("VARIABLES CHANGED");
      string dream = ((Ink.Runtime.StringValue) DialogueManager.GetInstance().GetVariableState("dream_choice")).value;
      string reason = ((Ink.Runtime.StringValue) DialogueManager.GetInstance().GetVariableState("reason_choice")).value;

      switch(dream)
      {
        case "Scholar":
        GameManager.Instance.PersistentGameplayData.MaxMana += 1;
        UIManager.Instance.InformationCanvas.SetActionPointText(GameManager.Instance.PersistentGameplayData.MaxMana);
        break;

        case "Adventurer":
        GameManager.Instance.PersistentGameplayData.proficiency += 2;
        UIManager.Instance.InformationCanvas.SetProficiencyText(GameManager.Instance.PersistentGameplayData.proficiency);
        break;

        case "Doctor":
       GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth += 50;
       // GameManager.Instance.PersistentGameplayData.bonusMaxHealth += value;
       UIManager.Instance.InformationCanvas.SetHealthText(GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth ,GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth);
        break;
      }

      switch(reason)
      {
        case "found":
        GameManager.Instance.PersistentGameplayData.DrawCount += 1;
        break;

        case "scared":
        GameManager.Instance.PersistentGameplayData.proficiency += 1;
        UIManager.Instance.InformationCanvas.SetProficiencyText(GameManager.Instance.PersistentGameplayData.proficiency);
        GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth += 25;
        UIManager.Instance.InformationCanvas.SetHealthText(GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth ,GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth);
        GameManager.Instance.PersistentGameplayData.LightLoss = 4;
        break;

        case "stay":
        GameManager.Instance.PersistentGameplayData.CurrentGold += 200;
        UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);
        break;

      }
    }



    IEnumerator Flow()
   {
      FadeBox.Play("FadeIn");

       yield return new WaitForSeconds(4);
       DialogueManager.GetInstance().EnterDialogueMode(scene1, backgroundAnimator, effectAnimator);
       GotoGame = true;


       

    }

     IEnumerator GameGo()
    {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene(scenename);

    }



}






