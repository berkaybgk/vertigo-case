namespace VertigoCase.Data
{
    // Immutable runtime record of a single reward earned during a spin.
    public sealed class CollectedReward
    {
        public RewardData RewardData    { get; }
        public int        Amount        { get; }
        public int        ZoneCollected { get; } // 1-based zone number

        public CollectedReward(RewardData rewardData, int amount, int zoneCollected)
        {
            RewardData    = rewardData;
            Amount        = amount;
            ZoneCollected = zoneCollected;
        }

        public override string ToString() =>
            $"{RewardData?.RewardName ?? "Unknown"} x{Amount} (Zone {ZoneCollected})";
    }
}
