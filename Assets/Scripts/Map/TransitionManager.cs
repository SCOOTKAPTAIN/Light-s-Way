using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;


public class TransitionManager : MonoBehaviour
{

    //public int ActNumber = 1;
    public Animator transistionanimator;
    public TextMeshProUGUI ActText;
    public TextMeshProUGUI ActDescription;
   
    
    void Start()
    {
        Debug.Log("initial value: " + GameManager.Instance.PersistentGameplayData.ActNumber);
        if(GameManager.Instance.PersistentGameplayData.ActNumber == 0)
        {
           StartCoroutine(Act1());
        }else
        {
            if(GameManager.Instance.PersistentGameplayData.actalreadyplayed == true)
            {
                return;

            }else
            {
                PlayAct();
            }
            
        }

        
    }


    void Update()
    {
        

    }

    

    IEnumerator Act1()
    {
        transistionanimator.Play("Act1");
        yield return new WaitForSeconds(1);
        DialogueAudioManager.instance.DynamicMusic("map");
        UIManager.Instance.InformationCanvas.gameObject.SetActive(true);
        GameManager.Instance.PersistentGameplayData.ActNumber++;
        Debug.Log("before intro" + GameManager.Instance.PersistentGameplayData.ActNumber);
    }

    public void PlayAct()
    {

        // 0 - start
        // 1 - Act 1
        // 2 - Act 1 Boss
        // 3 - Act 2
        // 4 - Act 2 Boss
        // 5 - Act 3
        // 6 - Act 3 Boss
        // 7 - Act 4 
        // 8 - Act 4 Boss
        // 9 - Final Act
        // 10 - Final Boss
        switch(GameManager.Instance.PersistentGameplayData.ActNumber)
        {
            case 3:
           

            ActText.text = "Chapter 2";
            ActDescription.text = "\"The life of my past, all asunder. I shall not look back, for I must push forward.\"";
            transistionanimator.Play("Act2");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 5:
           
            ActText.text = "Chapter 3";
            ActDescription.text = "\"Echoes of sorrow sings in my ear, but I won't crumble, for even in the darkest night, a spark of hope shall bring me light.\"";
            transistionanimator.Play("Act3");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 7:
           
            ActText.text = "Chapter 4";
            ActDescription.text = "\"Countless thoughts flood my mind; my heart grows weary and teary, yet I shall cling to the shadow of hope.\"";
            transistionanimator.Play("Act4");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 9:
           
            ActText.text = "Final Chapter";
            ActDescription.text = "\"Through torment and darkness, I persist, now my journey draws near it's end, an end to wandering, an end to fear.\"";
            transistionanimator.Play("Act5");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            default:
            break;
        }

    }
}
