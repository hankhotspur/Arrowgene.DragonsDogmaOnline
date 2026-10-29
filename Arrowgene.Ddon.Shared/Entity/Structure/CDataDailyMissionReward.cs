using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionReward
    {
        public uint Id { get; set; }
        public uint Value { get; set; }

        public class Serializer : EntitySerializer<CDataDailyMissionReward>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionReward obj)
            {
                WriteUInt32(buffer, obj.Id);
                WriteUInt32(buffer, obj.Value);
            }

            public override CDataDailyMissionReward Read(IBuffer buffer)
            {
                CDataDailyMissionReward obj = new CDataDailyMissionReward();

                obj.Id = ReadUInt32(buffer);
                obj.Value = ReadUInt32(buffer);

                return obj;
            }
        }
    }
}