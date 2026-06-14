using UnityEngine;

namespace VertigoCase.Data
{
    public enum ZoneType
    {
        Standard, // Zones 1-4, 6-9, 11-14, …  (bronze wheel, bomb present)
        Safe,     // Every 5th zone (silver wheel, no bomb)
        Super     // Zone 30 and 60 (gold wheel, no bomb)
    }

    // Configuration for a single zone in the 60-zone progression.
    [System.Serializable]
    public struct ZoneEntry
    {
        [Tooltip("1-based zone number for readability in the Inspector.")]
        [SerializeField] private int _zoneNumber;

        [Tooltip("Automatically derived from zone number — kept for Inspector readability only.")]
        [SerializeField] private ZoneType _zoneType;

        [Tooltip("Which wheel asset to spin at this zone.")]
        [SerializeField] private WheelConfigData _wheelConfig;

        [Tooltip("All slice amounts in this zone are multiplied by this factor.")]
        [SerializeField] private float _rewardMultiplier;

        [Tooltip("Gold required to revive if a bomb is hit at this zone. Should scale up with zone number.")]
        [SerializeField] private int _reviveCost;

        public int            ZoneNumber       => _zoneNumber;
        public ZoneType       ZoneType         => _zoneType;
        public WheelConfigData WheelConfig      => _wheelConfig;
        public float          RewardMultiplier => _rewardMultiplier;
        public int            ReviveCost       => _reviveCost;

        public ZoneEntry(int zoneNumber, ZoneType zoneType, WheelConfigData wheelConfig, float rewardMultiplier, int reviveCost)
        {
            _zoneNumber = zoneNumber;
            _zoneType = zoneType;
            _wheelConfig = wheelConfig;
            _rewardMultiplier = rewardMultiplier;
            _reviveCost = reviveCost;
        }
    }

    // ScriptableObject holding all zone configurations.
    [CreateAssetMenu(fileName = "ZoneConfigData", menuName = "VertigoCase/Zone Config")]
    public class ZoneConfigData : ScriptableObject
    {
        [Header("Wheel Configurations")]
        [SerializeField] private WheelConfigData _bronzeWheel;
        [SerializeField] private WheelConfigData _silverWheel;
        [SerializeField] private WheelConfigData _goldWheel;

        [Header("Progression Settings")]
        [SerializeField] private int _totalZones = 60;

        [Header("Multiplier Curve Settings")]
        [SerializeField] private float _baseMultiplier = 1f;
        [SerializeField] private float _multiplierIncrementPerZone = 1f;
        [SerializeField] private float _safeZoneMultiplierBonus = 0f;
        [SerializeField] private float _superZoneMultiplierBonus = 0f;

        [Header("Revive Cost Curve Settings")]
        [SerializeField] private int _baseReviveCost = 10;
        [SerializeField] private int _reviveCostIncrement = 10;

        public int TotalZones => _totalZones;

        public ZoneEntry[] Zones
        {
            get
            {
                ZoneEntry[] result = new ZoneEntry[_totalZones];
                for (int i = 0; i < _totalZones; i++)
                {
                    result[i] = GetZone(i);
                }
                return result;
            }
        }

        // Returns the zone entry at the given 0-based index.
        public ZoneEntry GetZone(int zeroBasedIndex)
        {
            if (zeroBasedIndex < 0 || zeroBasedIndex >= _totalZones)
                throw new System.IndexOutOfRangeException(
                    $"[ZoneConfigData] Zone index {zeroBasedIndex} is out of range (total={TotalZones}).");

            int zoneNumber = zeroBasedIndex + 1;
            ZoneType type = DetermineZoneType(zoneNumber);

            WheelConfigData wheelConfig = type switch
            {
                ZoneType.Standard => _bronzeWheel,
                ZoneType.Safe => _silverWheel,
                ZoneType.Super => _goldWheel,
                _ => _bronzeWheel
            };

            float multiplier = _baseMultiplier + (zoneNumber - 1) * _multiplierIncrementPerZone;
            if (type == ZoneType.Safe) multiplier += _safeZoneMultiplierBonus;
            if (type == ZoneType.Super) multiplier += _superZoneMultiplierBonus;

            int reviveCost = _baseReviveCost + (zoneNumber - 1) * _reviveCostIncrement;

            return new ZoneEntry(zoneNumber, type, wheelConfig, multiplier, reviveCost);
        }

        // ── Static helpers ─────────────────────────────────────────────────────

        // Returns the ZoneType for a 1-based zone number.
        public static ZoneType DetermineZoneType(int oneBasedZoneNumber)
        {
            if (oneBasedZoneNumber % 30 == 0)
                return ZoneType.Super;

            if (oneBasedZoneNumber % 5 == 0)
                return ZoneType.Safe;

            return ZoneType.Standard;
        }

        // ── Editor validation ──────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_bronzeWheel == null)
                Debug.LogWarning($"[ZoneConfigData] Bronze Wheel is not assigned in '{name}'.", this);
            if (_silverWheel == null)
                Debug.LogWarning($"[ZoneConfigData] Silver Wheel is not assigned in '{name}'.", this);
            if (_goldWheel == null)
                Debug.LogWarning($"[ZoneConfigData] Gold Wheel is not assigned in '{name}'.", this);
        }
#endif
    }
}
