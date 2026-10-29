using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Asset;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.GameServer.Handler
{
    public class DailyMissionListGetHandler : GameRequestPacketHandler<C2SDailyMissionListGetReq, S2CDailyMissionListGetRes>
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(DailyMissionListGetHandler));

        public DailyMissionListGetHandler(DdonGameServer server) : base(server)
        {
        }

        public override S2CDailyMissionListGetRes Handle(GameClient client, C2SDailyMissionListGetReq request)
        {
            S2CDailyMissionListGetRes res = new S2CDailyMissionListGetRes();

            // Read through the property each time rather than caching it, so an
            // edited JSON is picked up on the next request without a restart.
            DailyMissionAsset asset = Server.AssetRepository.DailyMissionAsset;
            if (asset == null)
            {
                Logger.Error(client, "DailyMissionAsset is not loaded; returning an empty mission list.");
                return res;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow.ToOffset(Server.GameSettings.GameServerSettings.GetEffectiveUtcOffset());

			uint characterId = client.Character.CharacterId;
            Dictionary<uint, DailyMissionProgress> progress = Server.Database.SelectRewardMissionProgress(characterId);

            Populate(res.DailyMissionList, asset, asset.DailyMissions, progress, now);
            Populate(res.SpecialRewardList, asset, asset.SpecialRewards, progress, now);

            // Time events (anniversary and similar) are not sent here. They use their
            // own packet, S2C_DAILY_MISSION_EVENT_LIST_GET_RES (72.3.2), and need
            // their own handler.

            return res;
        }

        private static void Populate(
            List<CDataDailyMission> target,
            DailyMissionAsset asset,
            List<DailyMission> missions,
            Dictionary<uint, DailyMissionProgress> progress,
            DateTimeOffset now)
        {
            foreach (var mission in missions)
            {
                var (begin, end) = asset.GetPeriod(mission.Period, now);

                uint missionProgress = 0;
                DailyMissionState state = DailyMissionState.InProgress;
                if (progress.TryGetValue(mission.MissionId, out DailyMissionProgress row))
                {
                    missionProgress = row.Progress;
                    state = row.State;
                }

                target.Add(mission.ToCData(begin, end, missionProgress, state));
            }
        }
    }
}
