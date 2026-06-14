using UnityEngine;

namespace VertigoCase.Data
{
// Defines one slice of a wheel. Embedded inside WheelConfigData.
    [System.Serializable]
    public class WheelSliceData
    {
        [Tooltip("The reward granted when this slice is landed on. Leave null if IsBomb is true.")]
        [SerializeField] private RewardData _reward;

        [Tooltip("Override amount for this specific slice (stacked with the zone multiplier at runtime).")]
        [SerializeField] private int _amount = 100;

        [Tooltip("When true, hitting this slice triggers the bomb/death flow instead of a reward.")]
        [SerializeField] private bool _isBomb;

        public RewardData Reward    => _reward;
        public int Amount           => _amount;
        public bool IsBomb          => _isBomb;

        public WheelSliceData() { }

        public WheelSliceData(RewardData reward, int amount, bool isBomb)
        {
            _reward = reward;
            _amount = amount;
            _isBomb = isBomb;
        }

#if UNITY_EDITOR
        // Surfaced only in the Editor to provide clearer Inspector labels.
        public string EditorLabel => _isBomb ? "💀 BOMB" : (_reward != null ? $"{_reward.RewardName} x{_amount}" : "⚠ No Reward Set");
#endif
    }
}
