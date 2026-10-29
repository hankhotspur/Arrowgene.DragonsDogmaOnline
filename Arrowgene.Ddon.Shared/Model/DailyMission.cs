namespace Arrowgene.Ddon.Shared.Model
{
    public enum DailyMissionObjective : uint
    {
        ClearQuest = 1,
        ClearDungeon = 2,
        ClearHardDungeon = 3,
        ClearBattleContent = 4,
        Other = 5,
        Craft = 6
    }

    public enum DailyMissionState : byte
    {
        Unavailable = 0,
        InProgress = 1,
        RewardAvailable = 2,
        RewardReceived = 3
    }

    public class DailyMissionProgress
    {
        public uint MissionId { get; set; }
        public uint Progress { get; set; }
        public DailyMissionState State { get; set; } = DailyMissionState.InProgress;
    }
}
