using Arrowgene.Ddon.Database.Model;
using Arrowgene.Ddon.GameServer.Characters;
using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Asset;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Arrowgene.Ddon.GameServer
{
    /// <summary>
    /// Tracks reward mission progress. Content handlers (quests, dungeons, crafting, ...)
    /// call ProgressMissions when the player does something that counts, and queue the
    /// returned packets.
    /// </summary>
    public class DailyMissionManager
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(DailyMissionManager));

        private readonly DdonGameServer _Server;

        public DailyMissionManager(DdonGameServer server)
        {
            _Server = server;
        }

        public PacketQueue ProgressMissions(GameClient client, DailyMissionObjective objective, uint amount = 1, DbConnection? connectionIn = null)
        {
            PacketQueue packets = new();

            DailyMissionAsset asset = _Server.AssetRepository.DailyMissionAsset;
            if (asset == null || amount == 0)
            {
                return packets;
            }

            List<DailyMission> missions = asset.DailyMissions.Where(x => x.Objective == (uint)objective).ToList();
            if (missions.Count == 0)
            {
                return packets;
            }

			uint characterId = client.Character.CharacterId;
            Dictionary<uint, DailyMissionProgress> stored = _Server.Database.SelectRewardMissionProgress(characterId, connectionIn);

            uint completedMissions = 0;
            foreach (DailyMission mission in missions)
            {
                DailyMissionProgress row = GetProgressRow(stored, mission.MissionId);
                if (!AdvanceMission(row, mission, amount, out bool completed))
                {
                    continue;
                }

                SaveProgressRow(characterId, row, connectionIn);
                if (completed)
                {
                    completedMissions++;
                }
            }

            if (completedMissions == 0)
            {
                return packets;
            }

            // Daily missions should count for special mission milestones
            foreach (DailyMission milestone in asset.SpecialRewards)
            {
                DailyMissionProgress row = GetProgressRow(stored, milestone.MissionId);
                if (AdvanceMission(row, milestone, completedMissions, out _))
                {
                    SaveProgressRow(characterId, row, connectionIn);
                }
            }

            // Tell the client which missions have a reward waiting (it shows the "!" on the NPC)
            HashSet<uint> knownIds = asset.DailyMissions.Concat(asset.SpecialRewards).Select(x => x.MissionId).ToHashSet();
            S2CDailyMissionRewardAvailableNtc ntc = new();
            foreach (DailyMissionProgress row in stored.Values.Where(x => x.State == DailyMissionState.RewardAvailable && knownIds.Contains(x.MissionId)))
            {
                ntc.MissionList.Add(new CDataCommonU32() { Value = row.MissionId });
            }

            client.Enqueue(ntc, packets);
            return packets;
        }

        public int ResetMissions(DailyMissionPeriod period)
        {
            DailyMissionAsset asset = _Server.AssetRepository.DailyMissionAsset;
            if (asset == null)
            {
                return 0;
            }

            List<uint> missionIds = asset.DailyMissions.Concat(asset.SpecialRewards)
                .Where(x => x.Period == period)
                .Select(x => x.MissionId)
                .ToList();

            int deleted = _Server.Database.DeleteRewardMissionProgress(missionIds);
            Logger.Info($"Reset {period} reward missions: cleared {deleted} progress rows.");
            return deleted;
        }

        private static DailyMissionProgress GetProgressRow(Dictionary<uint, DailyMissionProgress> stored, uint missionId)
        {
            if (!stored.TryGetValue(missionId, out DailyMissionProgress row))
            {
                row = new DailyMissionProgress() { MissionId = missionId };
                stored[missionId] = row;
            }

            return row;
        }

        // Returns true if the row changed. "completed" is true if this advance reached the goal.
        private static bool AdvanceMission(DailyMissionProgress row, DailyMission mission, uint amount, out bool completed)
        {
            completed = false;
            if (row.State != DailyMissionState.InProgress || amount == 0)
            {
                return false;
            }

            row.Progress = Math.Min(row.Progress + amount, mission.Goal);
            if (row.Progress >= mission.Goal)
            {
                row.State = DailyMissionState.RewardAvailable;
                completed = true;
            }

            return true;
        }

        private void SaveProgressRow(uint characterId, DailyMissionProgress row, DbConnection? connectionIn)
        {
            if (!_Server.Database.UpsertRewardMissionProgress(characterId, row, connectionIn))
            {
                Logger.Error($"Failed to store progress for daily mission {row.MissionId}");
            }
        }
    }
}
