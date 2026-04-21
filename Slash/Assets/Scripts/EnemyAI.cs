using UnityEngine;

namespace Slash
{
    // Fodder AI. Chases the player horizontally and does contact damage.
    // All movement and cooldowns respect TimeControl.enemyTimeScale so the
    // time stop ability can slow this enemy without touching Time.timeScale.
    [RequireComponent(typeof(Enemy))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;

        [Header("Movement")]
        public float moveSpeed = 3.5f;
        public float stopDistance = 0.6f;

        [Header("Contact Damage")]
        public int contactDamage = 1;
        public float contactCooldown = 0.7f;

        float _hitCooldownRemaining;
        Enemy _self;

        void Awake()
        {
            _self = GetComponent<Enemy>();
        }

        void Update()
        {
            if (player == null || _self == null || !_self.IsAlive) return;

            float dt = Time.deltaTime * TimeControl.enemyTimeScale;

            float dx = player.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);

            if (dist > stopDistance)
            {
                float step = Mathf.Sign(dx) * moveSpeed * dt;
                if (Mathf.Abs(step) > dist - stopDistance)
                {
                    step = Mathf.Sign(dx) * (dist - stopDistance);
                }
                transform.position += new Vector3(step, 0f, 0f);
            }

            if (_hitCooldownRemaining > 0f) _hitCooldownRemaining -= dt;

            if (playerHealth != null && dist <= stopDistance + 0.15f && _hitCooldownRemaining <= 0f)
            {
                playerHealth.TakeDamage(contactDamage);
                _hitCooldownRemaining = contactCooldown;
            }
        }
    }
}
