using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Blood splatter, screen flash, hit freeze, and screen shake on every hit.
    public class HitEffects : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController player;
        public CameraFollow cameraFollow;
        public Image flashOverlay;

        [Header("Screen Flash")]
        public Color flashColor = new Color(1f, 1f, 1f, 0.25f);
        public float flashDuration = 0.05f;

        [Header("Blood Splatter")]
        public int burstCount = 12;
        public float burstSpeed = 4f;
        public float particleLifetime = 0.4f;
        public float particleSize = 0.15f;
        public Color bloodColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        public float bloodGravity = 2f;

        [Header("Hit Freeze")]
        public float freezeDuration = 0.03f;

        ParticleSystem _bloodPS;
        float _flashUntil;
        float _freezeUntil;
        float _savedTimeScale;

        void Start()
        {
            if (player != null) player.OnHitLanded += OnHit;
            _bloodPS = CreateBloodSystem();
        }

        void OnDestroy()
        {
            if (player != null) player.OnHitLanded -= OnHit;
        }

        void Update()
        {
            if (flashOverlay != null)
            {
                if (Time.unscaledTime < _flashUntil)
                {
                    float t = 1f - ((_flashUntil - Time.unscaledTime) / flashDuration);
                    var c = flashColor;
                    c.a = Mathf.Lerp(flashColor.a, 0f, t);
                    flashOverlay.color = c;
                    flashOverlay.enabled = true;
                }
                else
                {
                    flashOverlay.enabled = false;
                }
            }

            if (_freezeUntil > 0f && Time.unscaledTime >= _freezeUntil)
            {
                Time.timeScale = _savedTimeScale;
                _freezeUntil = 0f;
            }
        }

        void OnHit(Vector3 position)
        {
            if (_bloodPS != null)
            {
                _bloodPS.transform.position = position;
                _bloodPS.Emit(burstCount);
            }

            _flashUntil = Time.unscaledTime + flashDuration;

            if (cameraFollow != null) cameraFollow.Shake();

            if (freezeDuration > 0f)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _freezeUntil = Time.unscaledTime + freezeDuration;
            }
        }

        ParticleSystem CreateBloodSystem()
        {
            var go = new GameObject("BloodSplatter");
            go.transform.SetParent(transform, worldPositionStays: false);

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = particleLifetime;
            main.startSpeed = burstSpeed;
            main.startSize = particleSize;
            main.startColor = bloodColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = bloodGravity;
            main.maxParticles = 200;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 5;

            return ps;
        }
    }
}
