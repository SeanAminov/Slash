using UnityEngine;

namespace Slash
{
    // Player HP. Nothing deals damage yet, but the infrastructure is in place
    // so enemies and projectiles can call TakeDamage once they are wired up.
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHP = 5;
        public float invulnOnHit = 0.4f;

        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;

        float _invulnUntil;
        SpriteRenderer _sr;
        Color _baseColor;
        float _flashUntil;

        void Awake()
        {
            CurrentHP = maxHP;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void Update()
        {
            if (_sr == null || _flashUntil <= 0f) return;

            if (Time.time < _flashUntil)
            {
                _sr.color = Color.red;
            }
            else
            {
                _sr.color = _baseColor;
                _flashUntil = 0f;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || Time.time < _invulnUntil) return;

            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            _invulnUntil = Time.time + invulnOnHit;
            _flashUntil = Time.time + 0.08f;
        }
    }
}
