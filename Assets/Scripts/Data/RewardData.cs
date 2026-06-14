using UnityEngine;

namespace VertigoCase.Data
{
    public enum RewardType
    {
        Currency,
        Chest,
        Weapon,
        Gear,
        Consumable
    }

    // Defines a single reward type.
    [CreateAssetMenu(fileName = "RewardData_New", menuName = "VertigoCase/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _rewardName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private RewardType _rewardType;

        [Header("Values")]
        [Tooltip("Base quantity (currency amount, item count, etc.)")]
        [SerializeField] private int _baseAmount = 1;

        [Tooltip("Rarity tier 1 = Common, 2 = Rare, 3 = Legendary")]
        [Range(1, 3)]
        [SerializeField] private int _tier = 1;

        public string RewardName  => _rewardName;
        public Sprite Icon        => _icon;
        public RewardType Type    => _rewardType;
        public int BaseAmount     => _baseAmount;
        public int Tier           => _tier;
    }
}
