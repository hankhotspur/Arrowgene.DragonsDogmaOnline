using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2SDailyMissionEventListGetReq : IPacketStructure
    {
        public PacketId Id => PacketId.C2S_DAILY_MISSION_EVENT_LIST_GET_REQ;

		public byte Unk0 { get; set; }

        public class Serializer : PacketEntitySerializer<C2SDailyMissionEventListGetReq>
        {
            public override void Write(IBuffer buffer, C2SDailyMissionEventListGetReq obj)
            {
                WriteByte(buffer, obj.Unk0);
            }

            public override C2SDailyMissionEventListGetReq Read(IBuffer buffer)
            {
                C2SDailyMissionEventListGetReq obj = new C2SDailyMissionEventListGetReq();

                obj.Unk0 = ReadByte(buffer);
                return obj;
            }
        }
    }
}
