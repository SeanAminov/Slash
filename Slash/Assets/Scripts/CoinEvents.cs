using System;

namespace Slash
{
    // Static bus so coins can publish pickups without direct references.
    public static class CoinEvents
    {
        public static event Action<int> OnCoinCollected;
        public static void RaisePickup(int value) { OnCoinCollected?.Invoke(value); }
    }
}
