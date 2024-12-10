using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Characters;

public class InkExternalFunctions
{
    public void Bind(Story story, Animator backgroundAnimator, Animator effectAnimator)
    {
      story.BindExternalFunction("Background", (string BgName) => Background(BgName, backgroundAnimator));
      story.BindExternalFunction("Music", (string MName) => ChangeMusic(MName));
     // story.BindExternalFunction("Flashback", (string state) => Flashback(state, FlashbackAnimator));
      story.BindExternalFunction("SE", (string SEName) => PlaySE(SEName));
      story.BindExternalFunction("Effect", (string EName) => Effect(EName, effectAnimator));
      story.BindExternalFunction("Light", (int value) => ChangeLight(value));
      story.BindExternalFunction("Health", (int value) => ChangeHealth(value));
      story.BindExternalFunction("Gold", (int value) => ChangeGold(value));
      story.BindExternalFunction("Proficiency", (int value) => ChangeProficiency(value));
      story.BindExternalFunction("MaxHealth", (int value) => ChangeMaxHealth(value));
      story.BindExternalFunction("IntroMaxHealth", (int value) => IntroMaxHealth(value));
      story.BindExternalFunction("AP", (int value) => ChangeMaxAP(value));
      story.BindExternalFunction("IntroConfirm", () => IntroConfirm());
      

    }

    public void Unbind(Story story) 
    {
      story.UnbindExternalFunction("Background");
      story.UnbindExternalFunction("Music");
      //story.UnbindExternalFunction("Flashback");
      story.UnbindExternalFunction("SE");
      story.UnbindExternalFunction("Effect");
      story.UnbindExternalFunction("Light");
      story.UnbindExternalFunction("Health");
      story.UnbindExternalFunction("Gold");
      story.UnbindExternalFunction("Proficiency");
      story.UnbindExternalFunction("MaxHealth");
      story.UnbindExternalFunction("IntroMaxHealth");
      story.UnbindExternalFunction("AP");
      story.UnbindExternalFunction("IntroConfirm");
      
      
    }

    public void ChangeLight(int value)
    {
      GameManager.Instance.PersistentGameplayData.ChangeLight(value);

      // GameManager.Instance.PersistentGameplayData.light += value;
      // UIManager.Instance.InformationCanvas.SetLightText(GameManager.Instance.PersistentGameplayData.light);
    }
    public void ChangeHealth(int value)
    {
      CombatManager.Instance.CurrentMainAlly.CharacterStats.Heal(Mathf.RoundToInt(value));
      GameManager.Instance.PersistentGameplayData.SetAllyHealthData(CombatManager.Instance.CurrentMainAlly.AllyCharacterData.CharacterID,CombatManager.Instance.CurrentMainAlly.CharacterStats.CurrentHealth,CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth);

     //UIManager.Instance.InformationCanvas.SetHealthText(CombatManager.Instance.CurrentMainAlly.CharacterStats.CurrentHealth,GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth);
    }
    public void ChangeGold(int value)
    {
      GameManager.Instance.PersistentGameplayData.CurrentGold += value;
      UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);
    }

    public void ChangeProficiency(int value)
    {
      GameManager.Instance.PersistentGameplayData.proficiency += value;
      UIManager.Instance.InformationCanvas.SetProficiencyText(GameManager.Instance.PersistentGameplayData.proficiency);
    }

    public void ChangeMaxAP(int value)
    {
      GameManager.Instance.PersistentGameplayData.MaxMana += value;
      UIManager.Instance.InformationCanvas.SetActionPointText(GameManager.Instance.PersistentGameplayData.MaxMana);
    }
    public void ChangeMaxHealth(int value)
    {
     
      CombatManager.Instance.CurrentMainAlly.CharacterStats.IncreaseMaxHealth(Mathf.RoundToInt(value));

      GameManager.Instance.PersistentGameplayData.SetAllyHealthData(CombatManager.Instance.CurrentMainAlly.AllyCharacterData.CharacterID
      ,CombatManager.Instance.CurrentMainAlly.CharacterStats.CurrentHealth + value,CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth);
      
     // UIManager.Instance.InformationCanvas.SetLightText(GameManager.Instance.PersistentGameplayData.light);
    }

    public void IntroMaxHealth(int value)
    {
      GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth += value;
     // GameManager.Instance.PersistentGameplayData.bonusMaxHealth += value;
      UIManager.Instance.InformationCanvas.SetHealthText(GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth ,GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth);

    }

    public void IntroConfirm()
    {
      string dream = ((Ink.Runtime.StringValue) DialogueManager.GetInstance().GetVariableState("dream_choice")).value;
      string reason = ((Ink.Runtime.StringValue) DialogueManager.GetInstance().GetVariableState("reason_choice")).value;

      switch(dream)
      {
        case "Scholar":
        ChangeMaxAP(1);
        break;

        case "Adventurer":
        ChangeProficiency(2);
        break;

        case "Doctor":
        IntroMaxHealth(50);
        break;
      }

      switch(reason)
      {
        case "found":
        GameManager.Instance.PersistentGameplayData.DrawCount += 1;
        break;

        case "scared":
        ChangeProficiency(1);
        IntroMaxHealth(25);
        GameManager.Instance.PersistentGameplayData.LightLoss = 4;
        break;

        case "stay":
        ChangeGold(200);
        break;

      }
    }

    







    public void Background(string BgName, Animator backgroundAnimator)
      {
        if( backgroundAnimator != null)
        {
          backgroundAnimator.Play(BgName);

        }
        else{
          Debug.LogWarning("Background Error lol");
        }
    
      }

      public void ChangeMusic(string MName)
    {
      if(MName == "stop")
      {
        DialogueAudioManager.instance.music_source.Stop();
      }
      else
      {
        DialogueAudioManager.instance.PlayMusic(MName);
      }
    }

    public void PlaySE(string SEName)
    {
     
      DialogueAudioManager.instance.PlaySFX(SEName);
    }

     public void Effect(string state, Animator effectAnimator)
    {
      effectAnimator.Play(state);
    }

    

    
    
}
