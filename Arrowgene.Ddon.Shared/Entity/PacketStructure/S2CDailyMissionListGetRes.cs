using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class S2CDailyMissionListGetRes : ServerResponse
    {
        public override PacketId Id => PacketId.S2C_DAILY_MISSION_LIST_GET_RES;

        public S2CDailyMissionListGetRes()
        {
            DailyMissionList = new List<CDataDailyMission>();
            SpecialRewardList = new List<CDataDailyMission>();
        }

        public List<CDataDailyMission> DailyMissionList { get; set; }
        public List<CDataDailyMission> SpecialRewardList { get; set; }

        public class Serializer : PacketEntitySerializer<S2CDailyMissionListGetRes>
        {
            public override void Write(IBuffer buffer, S2CDailyMissionListGetRes obj)
            {
                WriteServerResponse(buffer, obj);
                WriteEntityList(buffer, obj.DailyMissionList);
                WriteEntityList(buffer, obj.SpecialRewardList);
            }

            public override S2CDailyMissionListGetRes Read(IBuffer buffer)
            {
                S2CDailyMissionListGetRes obj = new();

                ReadServerResponse(buffer, obj);
                obj.DailyMissionList = ReadEntityList<CDataDailyMission>(buffer);
                obj.SpecialRewardList = ReadEntityList<CDataDailyMission>(buffer);

                return obj;
            }
        }
    }
}