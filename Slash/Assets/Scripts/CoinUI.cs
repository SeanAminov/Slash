using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Coin count readout with a pop scale on every pickup.
    public class CoinUI : MonoBehaviour
    {
        [Header("Wiring")]
        public CoinWallet wallet;
        public Text label;

        [Header("Pop")]
        public float popScale = 1.35f;
        public float popDuration = 0.2f;

        int _lastTotal = -1;
        float _popStart;
        Vector3 _baseScale = Vector3.one;
        RectTransform _rt;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (_rt != null) _baseScale = _rt.localScale;
        }

        void Update()
        {
            if (label == null || wallet == null) return;

            if (wallet.Total != _lastTotal)
            {
                _lastTotal = wallet.Total;
                _popStart = Time.time;
                label.text = _lastTotal.ToString();
            }

            if (_rt == null) return;

            float k = Mathf.Clamp01((Time.time - _popStart) / Mathf.Max(0.0001f, popDuration));
            float eased = 1f - k;
            float scale = 1f + eased * (popScale - 1f);
            _rt.localScale = _baseScale * scale;
        }
    }
}
