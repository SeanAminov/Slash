using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slash
{
    // Player movement and attacks. Each attack input is exactly one enemy hit.
    // Holding A or D on the press zips to the nearest enemy in that direction.
    // A clean press with no direction held is a neutral attack that stays on
    // the current target, used to chip down heavier enemies.
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 8f;

        [Header("Directional Zip")]
        public float zipRange = 14f;
        public float zipDuration = 0.07f;
        public float zipCooldown = 0.10f;
        public float zipLandOffset = 0.6f;
        public int zipDamage = 1;

        [Header("Neutral Attack")]
        public float neutralRange = 2.0f;
        public int neutralDamage = 1;
        public float neutralCooldown = 0.12f;

        [Header("Combo")]
        public float comboWindow = 2.0f;
        public float neutralMissGrace = 1.0f;

        // Fires when a hit connects. Passes the world position of the hit
        // and the direction from the player to the target.
        public event Action<Vector3, Vector2> OnHitLanded;

        float _attackReadyAt;
        bool _zipping;
        Enemy _currentTarget;
        int _comboCount;
        float _comboExpireAt;
        SlashTrail _trail;

        public int ComboCount => _comboCount;

        void Awake()
        {
            _trail = GetComponent<SlashTrail>();
        }

        void Update()
        {
            if (_zipping) return;

            HandleMovement();
            HandleAttack();
            TickCombo();
        }

        void HandleMovement()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float x = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;

            transform.position += new Vector3(x * moveSpeed * Time.deltaTime, 0f, 0f);
        }

        void HandleAttack()
        {
            if (!AttackPressed()) return;
            if (Time.time < _attackReadyAt) return;

            int dir = ReadHeldDirection();
            if (dir != 0) DoDirectionalZip(dir);
            else DoNeutralAttack();
        }

        bool AttackPressed()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            var kb = Keyboard.current;
            if (kb != null && kb.cKey.wasPressedThisFrame) return true;

            return false;
        }

        int ReadHeldDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return 0;

            bool left = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
            bool right = kb.dKey.isPressed || kb.rightArrowKey.isPressed;

            if (left && right) return 0;
            if (right) return 1;
            if (left) return -1;
            return 0;
        }

        void DoDirectionalZip(int dir)
        {
            var target = FindNearestInDirection(dir);
            if (target == null)
            {
                BreakCombo();
                _attackReadyAt = Time.time + zipCooldown;
                return;
            }
            StartCoroutine(ZipTo(target, dir));
        }

        IEnumerator ZipTo(Enemy target, int approachDir)
        {
            _zipping = true;
            Vector3 start = transform.position;
            Vector3 end = target.transform.position;

            // Land on the side we came from so we stay adjacent to this enemy.
            end.x -= approachDir * zipLandOffset;
            end.y = start.y;

            float t = 0f;
            while (t < zipDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t / zipDuration));
                if (_trail != null) _trail.SpawnGhost();
                yield return null;
            }
            transform.position = end;

            if (target != null && target.IsAlive)
            {
                target.TakeDamage(zipDamage);
                RegisterHit(target);
            }

            _attackReadyAt = Time.time + zipCooldown;
            _zipping = false;
        }

        void DoNeutralAttack()
        {
            Enemy target = null;

            if (_currentTarget != null && _currentTarget.IsAlive &&
                Vector2.Distance(transform.position, _currentTarget.transform.position) <= neutralRange)
            {
                target = _currentTarget;
            }
            else
            {
                target = FindNearestInRange(neutralRange);
            }

            if (target == null)
            {
                // Neutral miss shortens the combo window instead of killing it outright.
                if (_comboCount > 0)
                {
                    float graced = Time.time + neutralMissGrace;
                    if (graced < _comboExpireAt) _comboExpireAt = graced;
                }
                _attackReadyAt = Time.time + neutralCooldown;
                return;
            }

            target.TakeDamage(neutralDamage);
            RegisterHit(target);
            _attackReadyAt = Time.time + neutralCooldown;
        }

        Enemy FindNearestInDirection(int dir)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;
            float px = transform.position.x;

            foreach (var e in Enemy.All)
            {
                if (e == null || !e.IsAlive) continue;
                float dx = e.transform.position.x - px;
                if (dir > 0 && dx <= 0f) continue;
                if (dir < 0 && dx >= 0f) continue;

                float abs = Mathf.Abs(dx);
                if (abs > zipRange) continue;
                if (abs < bestDist)
                {
                    bestDist = abs;
                    best = e;
                }
            }
            return best;
        }

        Enemy FindNearestInRange(float range)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;
            foreach (var e in Enemy.All)
            {
                if (e == null || !e.IsAlive) continue;
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d > range) continue;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }
            return best;
        }

        void RegisterHit(Enemy target)
        {
            _currentTarget = target;
            _comboCount++;
            _comboExpireAt = Time.time + comboWindow;

            Vector2 dir = (Vector2)(target.transform.position - transform.position);
            if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(1f, 0f);
            else dir.Normalize();

            OnHitLanded?.Invoke(target.transform.position, dir);
        }

        void BreakCombo()
        {
            _comboCount = 0;
            _comboExpireAt = 0f;
        }

        void TickCombo()
        {
            if (_comboCount > 0 && Time.time >= _comboExpireAt)
            {
                _comboCount = 0;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, zipRange);
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.55f);
            Gizmos.DrawWireSphere(transform.position, neutralRange);
        }
    }
}
