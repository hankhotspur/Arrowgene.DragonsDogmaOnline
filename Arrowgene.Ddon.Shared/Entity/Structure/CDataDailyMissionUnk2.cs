using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionUnk2
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }

        public class Serializer : EntitySerializer<CDataDailyMissionUnk2>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionUnk2 obj)
            {
                WriteUInt64(buffer, obj.Unk0);
                WriteBool(buffer, obj.Unk1);
                WriteUInt32(buffer, obj.Unk2);
                WriteUInt32(buffer, obj.Unk3);
            }

            public override CDataDailyMissionUnk2 Read(IBuffer buffer)
            {
                CDataDailyMissionUnk2 obj = new CDataDailyMissionUnk2();

                obj.Unk0 = ReadUInt64(buffer);
                obj.Unk1 = ReadBool(buffer);
                obj.Unk2 = ReadUInt32(buffer);
                obj.Unk3 = ReadUInt32(buffer);

                return obj;
            }
        }
    }
}