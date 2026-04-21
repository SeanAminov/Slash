using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Basic enemy. Tracks HP and a flashing hit reaction. The invincible flag
    // lets the prototype keep a training dummy around for feel testing.
    // Self-registers in a static list so the player can query nearest cheaply.
    // Publishes OnAnyEnemyDied so coin spawners and ult systems can react.
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>();
        public static event Action<Vector3> OnAnyEnemyDied;

        [Header("Stats")]
        public int maxHP = 1;
        public bool invincible = false;

        public int CurrentHP { get; private set; }
        public bool IsAlive => invincible || CurrentHP > 0;

        SpriteRenderer _sr;
        Color _baseColor;
        float _flashUntil;

        void Awake()
        {
            CurrentHP = maxHP;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Update()
        {
            if (_sr == null || _flashUntil <= 0f) return;

            if (Time.time < _flashUntil)
            {
                _sr.color = Color.white;
            }
            else
            {
                _sr.color = _baseColor;
                _flashUntil = 0f;
            }
        }

        public void TakeDamage(int amount)
        {
            if (invincible)
            {
                Flash();
                return;
            }

            CurrentHP -= amount;
            Flash();

            if (CurrentHP <= 0) Die();
        }

        void Flash()
        {
            _flashUntil = Time.time + 0.06f;
        }

        void Die()
        {
            OnAnyEnemyDied?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }
}
