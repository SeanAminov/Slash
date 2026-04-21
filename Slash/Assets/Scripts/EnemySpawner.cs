using UnityEngine;

namespace Slash
{
    // Builds fodder enemies offscreen on a timer and walks them toward the
    // player. Everything is constructed in code so no prefab assets are needed.
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;

        [Header("Spawning")]
        public float spawnInterval = 1.6f;
        public float spawnDistance = 15f;
        public int maxAlive = 8;
        public float firstSpawnDelay = 1.5f;

        [Header("Fodder Look")]
        public Color fodderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public Vector2 fodderSize = new Vector2(0.9f, 1.4f);

        [Header("Fodder Stats")]
        public int fodderHP = 1;
        public float fodderSpeed = 3.5f;
        public int fodderContactDamage = 1;

        float _nextSpawnAt;
        static Sprite _squareSprite;

        void Start()
        {
            _nextSpawnAt = Time.time + firstSpawnDelay;
        }

        void Update()
        {
            if (player == null) return;
            if (Time.time < _nextSpawnAt) return;
            if (Enemy.All.Count >= maxAlive)
            {
                _nextSpawnAt = Time.time + 0.3f;
                return;
            }

            SpawnFodder();
            _nextSpawnAt = Time.time + spawnInterval;
        }

        void SpawnFodder()
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector3 pos = new Vector3(
                player.position.x + side * spawnDistance,
                player.position.y,
                0f);

            var go = new GameObject("Fodder");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(fodderSize.x, fodderSize.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSquareSprite();
            sr.color = fodderColor;
            sr.sortingOrder = 1;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.gravityScale = 0f;

            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = Vector2.one;

            var enemy = go.AddComponent<Enemy>();
            enemy.maxHP = fodderHP;

            var ai = go.AddComponent<EnemyAI>();
            ai.player = player;
            ai.playerHealth = playerHealth;
            ai.moveSpeed = fodderSpeed;
            ai.contactDamage = fodderContactDamage;
        }

        static Sprite GetSquareSprite()
        {
            if (_squareSprite != null) return _squareSprite;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            _squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _squareSprite;
        }
    }
}
