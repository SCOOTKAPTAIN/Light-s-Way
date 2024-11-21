using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Data.Settings;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.EnemyBehaviour;
using NueGames.NueDeck.Scripts.NueExtentions;
public class MyGameManager : MonoBehaviour
{

    

        [SerializeField] private TextMeshProUGUI goldTextField;
        [SerializeField] private TextMeshProUGUI healthTextField;

        public TextMeshProUGUI GoldTextField => goldTextField;

        public TextMeshProUGUI HealthTextField => healthTextField;

        public int currentHealth;
        public int maxHealth;
        public int currentGold;
    
    void Start()
    {
        
    }

   // public void SetGoldText(int value)=>GoldTextField.text = $"{value}";

   //  public void SetHealthText(int currentHealth,int maxHealth) => HealthTextField.text = $"{currentHealth}/{maxHealth}";

   
    void Update()
    {
        currentGold = GameManager.Instance.PersistentGameplayData.CurrentGold;
        maxHealth = GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth;
        //currentHealth = GameManager.Instance.PersistentGameplayData.AllyList[0].AllyCharacterData.CurrentHealth;
        UIChange();
        
    }

    public void UIChange()
    {
        goldTextField.text = currentGold.ToString();

        healthTextField.text = maxHealth.ToString();

    }
}
