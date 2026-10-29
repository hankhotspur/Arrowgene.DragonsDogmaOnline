public class WeeklyMissionResetTask : WeeklyTask
{
    public WeeklyMissionResetTask(DayOfWeek day, uint hour, uint minute)
        : base(TaskType.WeeklyMissionReset, day, hour, minute) { }

    public override void RunTask(DdonGameServer server)
    {
        // Mission progress only lives in the database and is read on every request, so the
        // head server can reset it directly. No RPC to the other channels is needed.
        server.DailyMissionManager.ResetMissions(Arrowgene.Ddon.Shared.Asset.DailyMissionPeriod.Weekly);
    }
}

return new WeeklyMissionResetTask(DayOfWeek.Monday, 5, 0);
