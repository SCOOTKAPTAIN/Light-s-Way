using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Characters
{
    public abstract class CharacterDataBase : ScriptableObject
    {
        [Header("Base")]
        [SerializeField] protected string characterID;
        [SerializeField] protected string characterName;
        [SerializeField] [TextArea] protected string characterDescription;
        [SerializeField] public int maxHealth;

        public string CharacterID => characterID;

        public string CharacterName => characterName;

        public string CharacterDescription => characterDescription;

       // public int MaxHealth => maxHealth;

       public int MaxHealth
       {
        get => maxHealth;
        set => maxHealth = value;
       }
    }
}