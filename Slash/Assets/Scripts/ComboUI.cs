using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // On screen combo display. Built to scale and feel punchy: a constant idle
    // pulse, a bigger pop on every combo increment, a much bigger pop when
    // crossing a milestone, and color tiers that escalate as the combo climbs.
    //
    // All tuning knobs are exposed so font, size, style, colors, and animation
    // speeds can be tweaked from the inspector without touching code.
    public class ComboUI : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController player;
        public Text label;

        [Header("Format")]
        public int minimumCombo = 1;     // display floor, always on screen at 1 or higher
        public string suffix = "x";

        [Header("Idle Pulse")]
        public float idlePulseAmount = 0.04f;
        public float idlePulseSpeed = 4f;

        [Header("Increment Punch")]
        public float incrementScale = 1.25f;
        public float incrementDuration = 0.12f;

        [Header("Milestone Punch")]
        public int[] milestones = { 10, 25, 50, 100 };
        public float milestoneScale = 1.9f;
        public float milestoneDuration = 0.35f;

        [Header("Color Tiers")]
        public Color[] tierColors = {
            new Color(1.0f, 1.0f, 1.0f, 1f),   //   < 10
            new Color(1.0f, 0.85f, 0.35f, 1f), // >= 10
            new Color(1.0f, 0.55f, 0.25f, 1f), // >= 25
            new Color(1.0f, 0.3f, 0.3f, 1f),   // >= 50
            new Color(1.0f, 0.2f, 0.8f, 1f),   // >= 100
        };
        public int[] tierThresholds = { 10, 25, 50, 100 };

        int _lastCombo = -1;
        float _punchStart;
        float _punchDuration;
        float _punchScale;
        Vector3 _baseScale;
        RectTransform _rt;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (_rt != null) _baseScale = _rt.localScale;
            _punchDuration = incrementDuration;
            _punchScale = 1f;
        }

        void Start()
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
        }

        void Update()
        {
            if (label == null) return;

            int combo = player != null ? Mathf.Max(player.ComboCount, 0) : 0;
            int displayed = Mathf.Max(combo, minimumCombo);

            if (combo != _lastCombo)
            {
                bool milestoneCrossed = CrossedMilestone(_lastCombo, combo);
                _lastCombo = combo;

                if (combo > 0)
                {
                    _punchStart = Time.time;
                    _punchDuration = milestoneCrossed ? milestoneDuration : incrementDuration;
                    _punchScale = milestoneCrossed ? milestoneScale : incrementScale;
                }
            }

            label.text = displayed + suffix;
            label.color = PickColor(combo);

            if (_rt != null)
            {
                float idle = 1f + Mathf.Sin(Time.time * idlePulseSpeed) * idlePulseAmount;

                float punch = 1f;
                if (_punchDuration > 0f)
                {
                    float k = Mathf.Clamp01((Time.time - _punchStart) / _punchDuration);
                    float eased = 1f - k;
                    punch = 1f + eased * (_punchScale - 1f);
                }

                _rt.localScale = _baseScale * idle * punch;
            }
        }

        bool CrossedMilestone(int prev, int now)
        {
            if (milestones == null) return false;
            for (int i = 0; i < milestones.Length; i++)
            {
                int m = milestones[i];
                if (prev < m && now >= m) return true;
            }
            return false;
        }

        Color PickColor(int combo)
        {
            if (tierColors == null || tierColors.Length == 0) return Color.white;

            Color current = tierColors[0];
            int max = Mathf.Min(tierThresholds.Length, tierColors.Length - 1);
            for (int i = 0; i < max; i++)
            {
                if (combo >= tierThresholds[i]) current = tierColors[i + 1];
            }
            return current;
        }
    }
}
