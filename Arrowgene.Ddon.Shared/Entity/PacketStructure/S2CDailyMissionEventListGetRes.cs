using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class S2CDailyMissionEventListGetRes : ServerResponse
    {
        public override PacketId Id => PacketId.S2C_DAILY_MISSION_EVENT_LIST_GET_RES;

        public S2CDailyMissionEventListGetRes()
        {
            TimeEventList = new List<CDataDailyMission>();
        }

        public byte Unk0 { get; set; }
        public List<CDataDailyMission> TimeEventList { get; set; }

        public class Serializer : PacketEntitySerializer<S2CDailyMissionEventListGetRes>
        {
            public override void Write(IBuffer buffer, S2CDailyMissionEventListGetRes obj)
            {
                WriteServerResponse(buffer, obj);
                WriteByte(buffer, obj.Unk0);
                WriteEntityList(buffer, obj.TimeEventList);
            }

            public override S2CDailyMissionEventListGetRes Read(IBuffer buffer)
            {
                S2CDailyMissionEventListGetRes obj = new();

                ReadServerResponse(buffer, obj);
                obj.Unk0 = ReadByte(buffer);
                obj.TimeEventList = ReadEntityList<CDataDailyMission>(buffer);

                return obj;
            }
        }
    }
}