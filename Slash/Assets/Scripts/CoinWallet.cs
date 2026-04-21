using UnityEngine;

namespace Slash
{
    // Running total of coins this run. Subscribes to the static pickup bus.
    public class CoinWallet : MonoBehaviour
    {
        public int Total { get; private set; }

        void OnEnable() { CoinEvents.OnCoinCollected += Add; }
        void OnDisable() { CoinEvents.OnCoinCollected -= Add; }

        void Add(int amount) { Total += amount; }
    }
}
