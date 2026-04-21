using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Visual effects for time stop: an expanding ring at the player's position
    // and a colored screen tint that lingers for the slowdown duration.
    public class TimeStopVFX : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public Image screenTint;

        [Header("Shockwave")]
        public Color ringColor = new Color(0.5f, 0.8f, 1f, 0.85f);
        public float ringMaxRadius = 18f;
        public float ringDuration = 0.6f;
        public float ringThickness = 0.25f;
        public int ringSegments = 64;

        [Header("Screen Tint")]
        public Color tintColor = new Color(0.4f, 0.7f, 1f, 0.22f);
        public float tintFadeIn = 0.15f;
        public float tintFadeOut = 0.3f;

        float _tintActiveUntil;
        float _tintDuration;
        float _ringStart;
        bool _ringAlive;
        LineRenderer _ring;

        void Awake()
        {
            _ring = BuildRing();
            _ring.enabled = false;
        }

        void Update()
        {
            UpdateRing();
            UpdateTint();
        }

        public void Trigger(float duration)
        {
            _tintActiveUntil = Time.time + duration;
            _tintDuration = duration;
            _ringStart = Time.time;
            _ringAlive = true;
            _ring.enabled = true;
        }

        void UpdateRing()
        {
            if (!_ringAlive) return;

            float t = (Time.time - _ringStart) / ringDuration;
            if (t >= 1f)
            {
                _ring.enabled = false;
                _ringAlive = false;
                return;
            }

            float radius = Mathf.Lerp(0f, ringMaxRadius, t);
            float alpha = Mathf.Lerp(1f, 0f, t * t);
            DrawRing(radius, alpha);
        }

        void UpdateTint()
        {
            if (screenTint == null) return;

            float now = Time.time;
            float remaining = _tintActiveUntil - now;

            if (remaining <= -tintFadeOut)
            {
                screenTint.enabled = false;
                return;
            }

            float elapsedIn = _tintDuration - remaining;
            float fadeIn = Mathf.Clamp01(elapsedIn / Mathf.Max(0.0001f, tintFadeIn));
            float fadeOut = remaining > 0f ? 1f : Mathf.Clamp01(1f + remaining / Mathf.Max(0.0001f, tintFadeOut));
            float mix = Mathf.Min(fadeIn, fadeOut);

            var c = tintColor;
            c.a = tintColor.a * mix;
            screenTint.color = c;
            screenTint.enabled = mix > 0f;
        }

        LineRenderer BuildRing()
        {
            var go = new GameObject("TimeStopRing");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = ringSegments;
            lr.widthMultiplier = ringThickness;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = ringColor;
            lr.endColor = ringColor;
            lr.sortingOrder = 10;
            return lr;
        }

        void DrawRing(float radius, float alpha)
        {
            Vector3 center = player != null ? player.position : transform.position;
            for (int i = 0; i < _ring.positionCount; i++)
            {
                float a = (i / (float)_ring.positionCount) * Mathf.PI * 2f;
                _ring.SetPosition(i, new Vector3(
                    center.x + Mathf.Cos(a) * radius,
                    center.y + Mathf.Sin(a) * radius,
                    0f));
            }
            var c = ringColor;
            c.a = ringColor.a * alpha;
            _ring.startColor = c;
            _ring.endColor = c;
            _ring.widthMultiplier = ringThickness * (0.5f + alpha * 0.5f);
        }
    }
}
