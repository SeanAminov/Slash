using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Fading afterimage ghosts spawned during zips.
    public class SlashTrail : MonoBehaviour
    {
        [Header("Afterimage")]
        public Color trailColor = new Color(0.3f, 0.8f, 1f, 0.5f);
        public float fadeDuration = 0.2f;
        public float spawnInterval = 0.015f;
        public int sortingOrder = -1;

        float _nextSpawn;
        SpriteRenderer _sr;
        readonly List<Ghost> _ghosts = new List<Ghost>();

        struct Ghost
        {
            public SpriteRenderer renderer;
            public float spawnTime;
        }

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            for (int i = _ghosts.Count - 1; i >= 0; i--)
            {
                var ghost = _ghosts[i];
                float age = Time.time - ghost.spawnTime;

                if (age >= fadeDuration)
                {
                    Destroy(ghost.renderer.gameObject);
                    _ghosts.RemoveAt(i);
                    continue;
                }

                float alpha = Mathf.Lerp(trailColor.a, 0f, age / fadeDuration);
                var c = ghost.renderer.color;
                c.a = alpha;
                ghost.renderer.color = c;
            }
        }

        public void SpawnGhost()
        {
            if (_sr == null || Time.time < _nextSpawn) return;
            _nextSpawn = Time.time + spawnInterval;

            var go = new GameObject("TrailGhost");
            go.transform.position = transform.position;
            go.transform.localScale = transform.lossyScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sr.sprite;
            sr.color = trailColor;
            sr.sortingOrder = sortingOrder;

            _ghosts.Add(new Ghost { renderer = sr, spawnTime = Time.time });
        }
    }
}
