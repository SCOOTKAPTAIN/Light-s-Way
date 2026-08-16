using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;


public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }
    public bool IsBlockingInput { get; private set; }

    //public int ActNumber = 1;
    public Animator transistionanimator;
    public TextMeshProUGUI ActText;
    public TextMeshProUGUI ActDescription;

    private void Awake()
    {
        Instance = this;
    }

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
        IsBlockingInput = true;
        transistionanimator.Play("Act1");
        yield return new WaitForSeconds(1.5f);
        IsBlockingInput = false;
        DialogueAudioManager.instance.DynamicMusic("map");
        UIManager.Instance.InformationCanvas.gameObject.SetActive(true);
        GameManager.Instance.PersistentGameplayData.ActNumber++;
        Debug.Log("before intro" + GameManager.Instance.PersistentGameplayData.ActNumber);
    }

    public void PlayAct()
    {
        IsBlockingInput = true;
        StartCoroutine(ReleaseInputAfterDelay(1.5f));
        // Simplified act system:
        // 0 - start
        // 1 - Act 1 (includes normal encounters and bosses)
        // 2 - Act 2 (includes normal encounters and bosses)
        // 3 - Act 3
        // 4 - Act 4
        // 5 - Final Act
        switch(GameManager.Instance.PersistentGameplayData.ActNumber)
        {
            case 2:
           
            ActText.text = "Chapter 2";
            ActDescription.text = "\"The life of my past, all asunder. I shall not look back, for I must push forward.\"";
            transistionanimator.Play("Act2");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 3:
           
            ActText.text = "Chapter 3";
            ActDescription.text = "\"Echoes of sorrow sings in my ear, but I won't crumble, for even in the darkest night, a spark of hope shall bring me light.\"";
            transistionanimator.Play("Act3");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 4:
           
            ActText.text = "Chapter 4";
            ActDescription.text = "\"Countless thoughts flood my mind; my heart grows weary and teary, yet I shall cling to the shadow of hope.\"";
            transistionanimator.Play("Act4");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            case 5:
           
            ActText.text = "Final Chapter";
            ActDescription.text = "\"Through torment and darkness, I persist, now my journey draws near it's end, an end to wandering, an end to fear.\"";
            transistionanimator.Play("Act5");
            GameManager.Instance.PersistentGameplayData.actalreadyplayed = true;
            break;

            default:
            break;
        }

    }

    private IEnumerator ReleaseInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsBlockingInput = false;
    }
}
