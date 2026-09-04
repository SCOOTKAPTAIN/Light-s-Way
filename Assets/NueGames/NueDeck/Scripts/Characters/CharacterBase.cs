using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Interfaces;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Characters
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        [Header("Base settings")]
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Transform textSpawnRoot;

        #region Cache
        public CharacterStats CharacterStats { get; protected set; }
        public CharacterType CharacterType => characterType;
        public Transform TextSpawnRoot => textSpawnRoot;
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;
        protected UIManager UIManager => UIManager.Instance;

        #endregion
       

        public virtual void Awake()
        {
        }
        
        public virtual void BuildCharacter()
        {
            
        }
        
        protected virtual void OnDeath()
        {
            
        }
        
        public  CharacterBase GetCharacterBase()
        {
            return this;
        }

        public CharacterType GetCharacterType()
        {
            return CharacterType;
        }

        // Small hit jitter to give feedback when this character takes unblocked damage.
        private Coroutine _hitJitterCoroutine;
        private Vector3 _hitJitterBasePosition;
        public void PlayHitJitter(float intensity = 0.06f, float duration = 0.12f)
        {
            if (!gameObject.activeInHierarchy) return;

            if (_hitJitterCoroutine == null)
                _hitJitterBasePosition = transform.localPosition;

            if (_hitJitterCoroutine != null)
                StopCoroutine(_hitJitterCoroutine);
            _hitJitterCoroutine = StartCoroutine(HitJitterRoutine(intensity, duration));
        }

        private System.Collections.IEnumerator HitJitterRoutine(float intensity, float duration)
        {
            var original = _hitJitterBasePosition;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                var strength = Mathf.Lerp(intensity, 0f, t);

                // Bias jitter to be more horizontal: X larger, Y smaller, Z minimal
                var horizontal = UnityEngine.Random.Range(-1f, 1f) * 1.25f; // emphasize X
                var vertical = UnityEngine.Random.Range(-0.2f, 0.2f); // small Y movement
                var depth = UnityEngine.Random.Range(-0.05f, 0.05f); // minimal Z

                var offset = new Vector3(horizontal, vertical, depth) * strength;
                transform.localPosition = original + offset;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = original;
            _hitJitterBasePosition = original;
            _hitJitterCoroutine = null;
        }
    }
}