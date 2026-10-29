using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class S2CDailyMissionRewardAvailableNtc : IPacketStructure
    {
        public S2CDailyMissionRewardAvailableNtc()
        {
            MissionList = new List<CDataCommonU32>();
        }

        public List<CDataCommonU32> MissionList { get; set; }

        public PacketId Id => PacketId.S2C_DAILY_72_1_16_NTC;

        public class Serializer : PacketEntitySerializer<S2CDailyMissionRewardAvailableNtc>
        {
            public override void Write(IBuffer buffer, S2CDailyMissionRewardAvailableNtc obj)
            {
                WriteEntityList(buffer, obj.MissionList);
            }

            public override S2CDailyMissionRewardAvailableNtc Read(IBuffer buffer)
            {
                var obj = new S2CDailyMissionRewardAvailableNtc();
                obj.MissionList = ReadEntityList<CDataCommonU32>(buffer);
                return obj;
            }
        }
    }
}
