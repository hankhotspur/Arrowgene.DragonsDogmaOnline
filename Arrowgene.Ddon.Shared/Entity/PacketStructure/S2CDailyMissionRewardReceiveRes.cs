using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Network;

namespace Arrowgene.Ddon.Shared.Entity.PacketStructure
{
    public class S2CDailyMissionRewardReceiveRes : ServerResponse
    {
        public override PacketId Id => PacketId.S2C_DAILY_MISSION_REWARD_RECEIVE_RES;

        public S2CDailyMissionRewardReceiveRes()
        {
            UpdateItemList = new List<CDataItemUpdateResult>();
            UpdateWalletList = new List<CDataUpdateWalletPoint>();
        }

        public List<CDataItemUpdateResult> UpdateItemList { get; set; }
        public List<CDataUpdateWalletPoint> UpdateWalletList { get; set; }

        public class Serializer : PacketEntitySerializer<S2CDailyMissionRewardReceiveRes>
        {
            public override void Write(IBuffer buffer, S2CDailyMissionRewardReceiveRes obj)
            {
                WriteServerResponse(buffer, obj);
                WriteEntityList<CDataItemUpdateResult>(buffer, obj.UpdateItemList);
                WriteEntityList<CDataUpdateWalletPoint>(buffer, obj.UpdateWalletList);
            }

            public override S2CDailyMissionRewardReceiveRes Read(IBuffer buffer)
            {
                S2CDailyMissionRewardReceiveRes obj = new();

                ReadServerResponse(buffer, obj);
                obj.UpdateItemList = ReadEntityList<CDataItemUpdateResult>(buffer);
                obj.UpdateWalletList = ReadEntityList<CDataUpdateWalletPoint>(buffer);

                return obj;
            }
        }
    }
}