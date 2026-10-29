using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionDetail
    {
        public uint MissionId { get; set; }
        public uint Unk0 { get; set; } // Always 999 or 0
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
        public List<CDataDailyMissionReward> ItemReward { get; set; } = [];
        public List<CDataDailyMissionReward> WalletReward { get; set; } = [];
        public string RewardCode { get; set; } = string.Empty; // Possible mail title
        public List<CDataDailyMissionUnk1> UnkList1 { get; set; } = []; // Empty?
        public List<CDataDailyMissionUnk2> UnkList2 { get; set; } = []; // Mail: Golden gemstone
        public List<CDataDailyMissionUnk3> UnkList3 { get; set; } = []; // Crashes
        public List<CDataDailyMissionUnk4> UnkList4 { get; set; } = []; // Mail: Craft master

        public class Serializer : EntitySerializer<CDataDailyMissionDetail>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionDetail obj)
            {
                WriteUInt32(buffer, obj.MissionId);
                WriteUInt32(buffer, obj.Unk0);
                WriteInt64(buffer, obj.Begin.ToUnixTimeSeconds());
                WriteInt64(buffer, obj.End.ToUnixTimeSeconds());
                WriteEntityList(buffer, obj.ItemReward);
                WriteEntityList(buffer, obj.WalletReward);
                WriteMtString(buffer, obj.RewardCode);
                WriteEntityList(buffer, obj.UnkList1);
                WriteEntityList(buffer, obj.UnkList2);
                WriteEntityList(buffer, obj.UnkList3);
                WriteEntityList(buffer, obj.UnkList4);
            }

            public override CDataDailyMissionDetail Read(IBuffer buffer)
            {
                CDataDailyMissionDetail obj = new CDataDailyMissionDetail();

                obj.MissionId = ReadUInt32(buffer);
                obj.Unk0 = ReadUInt32(buffer);
                obj.Begin = DateTimeOffset.FromUnixTimeSeconds(ReadInt64(buffer));
                obj.End = DateTimeOffset.FromUnixTimeSeconds(ReadInt64(buffer));
                obj.ItemReward = ReadEntityList<CDataDailyMissionReward>(buffer);
                obj.WalletReward = ReadEntityList<CDataDailyMissionReward>(buffer);
                obj.RewardCode = ReadMtString(buffer);
                obj.UnkList1 = ReadEntityList<CDataDailyMissionUnk1>(buffer);
                obj.UnkList2 = ReadEntityList<CDataDailyMissionUnk2>(buffer);
                obj.UnkList3 = ReadEntityList<CDataDailyMissionUnk3>(buffer);
                obj.UnkList4 = ReadEntityList<CDataDailyMissionUnk4>(buffer);

                return obj;
            }
        }
    }
}