using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionUnk4
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public string Unk3 { get; set; } = string.Empty;

        public class Serializer : EntitySerializer<CDataDailyMissionUnk4>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionUnk4 obj)
            {
                WriteUInt64(buffer, obj.Unk0);
                WriteBool(buffer, obj.Unk1);
                WriteUInt32(buffer, obj.Unk2);
                WriteMtString(buffer, obj.Unk3);
            }

            public override CDataDailyMissionUnk4 Read(IBuffer buffer)
            {
                CDataDailyMissionUnk4 obj = new CDataDailyMissionUnk4();

                obj.Unk0 = ReadUInt64(buffer);
                obj.Unk1 = ReadBool(buffer);
                obj.Unk2 = ReadUInt32(buffer);
                obj.Unk3 = ReadMtString(buffer);

                return obj;
            }
        }
    }
}