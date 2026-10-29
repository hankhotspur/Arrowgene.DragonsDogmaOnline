using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMission
    {
        public uint MissionId { get; set; }
        public uint Objective { get; set; }
        public uint Goal { get; set; }
        public CDataDailyMissionDetail MissionDetail { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public uint Progress { get; set; }
        public byte State { get; set; }
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
        public uint SortNo { get; set; }

        public class Serializer : EntitySerializer<CDataDailyMission>
        {
            public override void Write(IBuffer buffer, CDataDailyMission obj)
            {
                WriteUInt32(buffer, obj.MissionId);
                WriteUInt32(buffer, obj.Objective);
                WriteUInt32(buffer, obj.Goal);
                WriteEntity<CDataDailyMissionDetail>(buffer, obj.MissionDetail);
                WriteMtString(buffer, obj.Title);
                WriteMtString(buffer, obj.IconUrl);
                WriteMtString(buffer, obj.ImageUrl);
                WriteUInt32(buffer, obj.Progress);
                WriteByte(buffer, obj.State);
                WriteInt64(buffer, obj.Begin.ToUnixTimeSeconds());
                WriteInt64(buffer, obj.End.ToUnixTimeSeconds());
                WriteUInt32(buffer, obj.SortNo);
            }

            public override CDataDailyMission Read(IBuffer buffer)
            {
                CDataDailyMission obj = new CDataDailyMission();

                obj.MissionId = ReadUInt32(buffer);
                obj.Objective = ReadUInt32(buffer);
                obj.Goal = ReadUInt32(buffer);
                obj.MissionDetail = ReadEntity<CDataDailyMissionDetail>(buffer);
                obj.Title = ReadMtString(buffer);
                obj.IconUrl = ReadMtString(buffer);
                obj.ImageUrl = ReadMtString(buffer);
                obj.Progress = ReadUInt32(buffer);
                obj.State = ReadByte(buffer);
                obj.Begin = DateTimeOffset.FromUnixTimeSeconds(ReadInt64(buffer));
                obj.End = DateTimeOffset.FromUnixTimeSeconds(ReadInt64(buffer));
                obj.SortNo = ReadUInt32(buffer);

                return obj;
            }
        }
    }
}