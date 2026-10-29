using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Network;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class C2SDailyMissionRewardReceiveReq : IPacketStructure
    {
        public PacketId Id => PacketId.C2S_DAILY_MISSION_REWARD_RECEIVE_REQ;

        public C2SDailyMissionRewardReceiveReq()
        {
            MissionIdList = new List<uint>();
        }

        public List<uint> MissionIdList { get; set; }
        public byte StorageType { get; set; }

        public class Serializer : PacketEntitySerializer<C2SDailyMissionRewardReceiveReq>
        {
            public override void Write(IBuffer buffer, C2SDailyMissionRewardReceiveReq obj)
            {
                WriteUInt32(buffer, (uint)obj.MissionIdList.Count);
                foreach (var missionId in obj.MissionIdList)
                {
                    WriteUInt32(buffer, missionId);
                }
                WriteByte(buffer, obj.StorageType);
            }

            public override C2SDailyMissionRewardReceiveReq Read(IBuffer buffer)
            {
                C2SDailyMissionRewardReceiveReq obj = new C2SDailyMissionRewardReceiveReq();

                uint count = ReadUInt32(buffer);
                for (uint i = 0; i < count; i++)
                {
                    obj.MissionIdList.Add(ReadUInt32(buffer));
                }
                obj.StorageType = ReadByte(buffer);

                return obj;
            }
        }
    }
}
