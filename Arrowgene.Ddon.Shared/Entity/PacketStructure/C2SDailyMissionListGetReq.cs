using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2SDailyMissionListGetReq : IPacketStructure
    {
        public PacketId Id => PacketId.C2S_DAILY_MISSION_LIST_GET_REQ;

        public class Serializer : PacketEntitySerializer<C2SDailyMissionListGetReq>
        {
            public override void Write(IBuffer buffer, C2SDailyMissionListGetReq obj)
            {
            }

            public override C2SDailyMissionListGetReq Read(IBuffer buffer)
            {
                C2SDailyMissionListGetReq obj = new C2SDailyMissionListGetReq();
                return obj;
            }
        }
    }
}
