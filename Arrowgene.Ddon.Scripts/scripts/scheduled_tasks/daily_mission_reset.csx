public class DailyMissionResetTask : DailyTask
{
    public DailyMissionResetTask(uint hour, uint minute)
        : base(TaskType.DailyMissionReset, hour, minute) { }

    public override void RunTask(DdonGameServer server)
    {
        // Mission progress only lives in the database and is read on every request, so the
        // head server can reset it directly. No RPC to the other channels is needed.
        server.DailyMissionManager.ResetMissions(Arrowgene.Ddon.Shared.Asset.DailyMissionPeriod.Daily);
    }
}

return new DailyMissionResetTask(5, 0);
