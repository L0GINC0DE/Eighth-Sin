using System;
using UnityEngine;
using UnityEngine.Events;

    public class HealthModule : MonoBehaviour
    {
        public event Action OnHit;
        public event Action OnDead;

        [SerializeField] private int maxHealth = 50;
        [SerializeField] private int currentHealth;
        private bool initialized;

        public int MaxHealth => maxHealth;
        public int CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return currentHealth;
            }
        }
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void ResetHealth()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            initialized = true;
            SetHealth(maxHealth);
        }

        private void EnsureInitialized()
        {
            if (!initialized) ResetHealth();
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            SetHealth(currentHealth + Mathf.Min(amount, maxHealth - currentHealth));
        }
        
        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;

            int previous = currentHealth;
            SetHealth(currentHealth - damage, true);
        }

        private void SetHealth(int value, bool isHit = false)
        {
            int previous = currentHealth;
            int next = Mathf.Clamp(value, 0, maxHealth);

            if (previous == next)
                return;

            currentHealth = next;
            if (isHit) OnHit?.Invoke();

            if (previous > 0 && next == 0)
                OnDead?.Invoke();
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            float healthRatio = currentHealth / (float)maxHealth;
            int newMaxHealth = Mathf.RoundToInt(maxHealth * (1f / 100f));
            int newCurrentHealth = (int)(newMaxHealth * healthRatio);
            currentHealth = newCurrentHealth;
        }
    }
