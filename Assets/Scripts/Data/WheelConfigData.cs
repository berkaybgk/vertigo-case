using UnityEngine;

namespace VertigoCase.Data
{
    public enum WheelType
    {
        Standard, // Bronze – bomb present
        Safe,     // Silver – no bomb
        Super     // Gold   – no bomb, special rewards
    }

    [System.Serializable]
    public class RewardPoolEntry
    {
        [SerializeField] private RewardData _reward;
        [SerializeField] private int _weight = 100;
        [SerializeField] private int _minAmount = 1;
        [SerializeField] private int _maxAmount = 10;
        [SerializeField] private int _minZone = 1;
        [SerializeField] private int _maxZone = 60;
        public RewardData Reward    => _reward;
        public int        Weight    => _weight;
        public int        MinAmount => _minAmount;
        public int        MaxAmount => _maxAmount;
        public int        MinZone   => _minZone;
        public int        MaxZone   => _maxZone;
    }

    // Configuration for one wheel variant (Standard / Safe / Super).
    [CreateAssetMenu(fileName = "WheelConfig_New", menuName = "VertigoCase/Wheel Config")]
    public class WheelConfigData : ScriptableObject
    {
        // ── Wheel identity ─────────────────────────────────────────────────────
        [Header("Wheel Type")]
        [SerializeField] private WheelType _wheelType;

        [Header("Dynamic Generation Pool")]
        [SerializeField] private RewardPoolEntry[] _rewardPool;

        // ── Visual assets ──────────────────────────────────────────────────────
        [Header("Visuals")]
        [Tooltip("The circular base image (bronze / silver / gold).")]
        [SerializeField] private Sprite _wheelBase;

        [Tooltip("The fixed arrow/indicator pointing to the winning slice.")]
        [SerializeField] private Sprite _wheelIndicator;

        // ── Spin feel ──────────────────────────────────────────────────────────
        [Header("Spin Settings")]
        [Tooltip("Total duration of the spin animation in seconds.")]
        [SerializeField] private float _spinDuration = 4f;

        [Tooltip("Minimum number of full 360° rotations before decelerating.")]
        [SerializeField] private int _minRotations = 4;

        // ── Public API ─────────────────────────────────────────────────────────
        public WheelType   WheelType     => _wheelType;
        public Sprite      WheelBase     => _wheelBase;
        public Sprite      WheelIndicator => _wheelIndicator;
        public float       SpinDuration  => _spinDuration;
        public int         MinRotations  => _minRotations;

        // Fixed at 8 — matches the provided wheel sprite asset which has exactly 8 segments.
        // Update this constant (and re-test all spin maths) if the art ever changes.
        private const int k_SliceCount = 8;
        public int   SliceCount    => k_SliceCount;
        public float SliceAngleDeg => 360f / k_SliceCount;

        public WheelSliceData[] GetSlicesForZone(int zoneNumber)
        {
            if (_rewardPool == null || _rewardPool.Length == 0)
            {
                Debug.LogError($"[WheelConfigData] '{name}' has an empty reward pool! Cannot generate slices.");
                return new WheelSliceData[k_SliceCount];
            }

            var activePool = new System.Collections.Generic.List<RewardPoolEntry>();
            foreach (var entry in _rewardPool)
            {
                if (entry.Reward != null && zoneNumber >= entry.MinZone && zoneNumber <= entry.MaxZone)
                {
                    activePool.Add(entry);
                }
            }

            if (activePool.Count == 0)
            {
                foreach (var entry in _rewardPool)
                {
                    if (entry.Reward != null) activePool.Add(entry);
                }
            }

            if (activePool.Count == 0)
            {
                Debug.LogError($"[WheelConfigData] No valid rewards found in the pool for '{name}'.");
                return new WheelSliceData[k_SliceCount];
            }

            // Seed deterministic random using zone number and name hash
            int seed = name.GetHashCode() ^ zoneNumber;
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed);

            int count = k_SliceCount;
            WheelSliceData[] generatedSlices = new WheelSliceData[count];

            // Randomly select the bomb slice index deterministically for this zone
            int bombSliceIndex = UnityEngine.Random.Range(0, count);

            for (int i = 0; i < count; i++)
            {
                if (_wheelType == WheelType.Standard && i == bombSliceIndex)
                {
                    generatedSlices[i] = new WheelSliceData(null, 0, true);
                }
                else
                {
                    RewardPoolEntry chosen = GetWeightedRandomReward(activePool);
                    int amount = UnityEngine.Random.Range(chosen.MinAmount, chosen.MaxAmount + 1);
                    generatedSlices[i] = new WheelSliceData(chosen.Reward, amount, false);
                }
            }

            UnityEngine.Random.state = oldState;
            return generatedSlices;
        }

        private RewardPoolEntry GetWeightedRandomReward(System.Collections.Generic.List<RewardPoolEntry> pool)
        {
            int totalWeight = 0;
            foreach (var entry in pool)
            {
                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return pool[UnityEngine.Random.Range(0, pool.Count)];
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var entry in pool)
            {
                cumulative += entry.Weight;
                if (roll < cumulative)
                {
                    return entry;
                }
            }

            return pool[pool.Count - 1];
        }

        // ── Editor validation ──────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rewardPool == null || _rewardPool.Length == 0)
            {
                Debug.LogWarning($"[WheelConfigData] '{name}' has no items configured in its reward pool.", this);
            }
        }
#endif
    }
}
