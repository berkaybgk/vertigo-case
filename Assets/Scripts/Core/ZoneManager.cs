using UnityEngine;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    // Manages zone progression.
    public class ZoneManager : MonoBehaviour
    {
        [SerializeField] private ZoneConfigData _masterConfig;

        // Current zone as a 0-based index.
        public int CurrentZoneIndex { get; private set; } = 0;

        public int TotalZones => _masterConfig != null ? _masterConfig.TotalZones : 0;

        // ── Public API ─────────────────────────────────────────────────────────

        public ZoneEntry GetCurrentZone() => _masterConfig.GetZone(CurrentZoneIndex);

        // 1-based zone number for display (e.g., "Zone 5").
        public int GetCurrentZoneNumber() => CurrentZoneIndex + 1;

        // Increments the zone index. Returns true if there are more zones remaining.
        public bool AdvanceZone()
        {
            if (CurrentZoneIndex >= TotalZones - 1)
                return false;

            CurrentZoneIndex++;
            return true;
        }

        public void ResetToStart() => CurrentZoneIndex = 0;

        // ── Editor validation ──────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_masterConfig == null)
                Debug.LogWarning("[ZoneManager] No ZoneConfigData assigned.", this);
        }
#endif
    }
}
