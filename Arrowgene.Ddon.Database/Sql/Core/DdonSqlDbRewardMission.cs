#nullable enable
using System.Collections.Generic;
using System.Data.Common;
using Arrowgene.Ddon.Shared.Model;

namespace Arrowgene.Ddon.Database.Sql.Core;

public partial class DdonSqlDb : SqlDb
{
    private static readonly string[] RewardMissionProgressFields = new[]
    {
        "character_id", "mission_id", "progress", "state"
    };

    private readonly string SqlSelectRewardMissionProgress =
        $"SELECT {BuildQueryField(RewardMissionProgressFields)} FROM \"ddon_reward_mission_progress\" WHERE \"character_id\" = @character_id;";

    private readonly string SqlUpsertRewardMissionProgress =
        $"""
        INSERT INTO "ddon_reward_mission_progress" ({BuildQueryField(RewardMissionProgressFields)})
            VALUES ({BuildQueryInsert(RewardMissionProgressFields)})
            ON CONFLICT ("character_id", "mission_id")
            DO UPDATE SET "progress" = EXCLUDED."progress", "state" = EXCLUDED."state";
        """;

    public override Dictionary<uint, DailyMissionProgress> SelectRewardMissionProgress(uint characterId, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            Dictionary<uint, DailyMissionProgress> progress = new();
            ExecuteReader(connection, SqlSelectRewardMissionProgress,
                command =>
                {
                    AddParameter(command, "@character_id", characterId);
                },
                reader =>
                {
                    while (reader.Read())
                    {
                        DailyMissionProgress row = new()
                        {
                            MissionId = GetUInt32(reader, "mission_id"),
                            Progress = GetUInt32(reader, "progress"),
                            State = (DailyMissionState)GetByte(reader, "state")
                        };
                        progress[row.MissionId] = row;
                    }
                });

            return progress;
        });
    }

    public override bool UpsertRewardMissionProgress(uint characterId, DailyMissionProgress progress, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            int rowsAffected = ExecuteNonQuery(connection, SqlUpsertRewardMissionProgress, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@mission_id", progress.MissionId);
                AddParameter(command, "@progress", progress.Progress);
                AddParameter(command, "@state", (byte)progress.State);
            });

            return rowsAffected > NoRowsAffected;
        });
    }

    public override int DeleteRewardMissionProgress(IReadOnlyCollection<uint> missionIds, DbConnection? connectionIn = null)
    {
        if (missionIds.Count == 0)
        {
            return 0;
        }

        string idList = string.Join(",", missionIds);
        string sql = $"DELETE FROM \"ddon_reward_mission_progress\" WHERE \"state\" <> {(byte)DailyMissionState.Unavailable} AND \"mission_id\" IN ({idList});";

        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection, sql, command => { });
        });
    }
}
