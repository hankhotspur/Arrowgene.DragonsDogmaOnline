using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionUnk3
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }

        public class Serializer : EntitySerializer<CDataDailyMissionUnk3>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionUnk3 obj)
            {
                WriteUInt64(buffer, obj.Unk0);
                WriteBool(buffer, obj.Unk1);
                WriteUInt32(buffer, obj.Unk2);
                WriteUInt32(buffer, obj.Unk3);
                WriteUInt32(buffer, obj.Unk4);
            }

            public override CDataDailyMissionUnk3 Read(IBuffer buffer)
            {
                CDataDailyMissionUnk3 obj = new CDataDailyMissionUnk3();

                obj.Unk0 = ReadUInt64(buffer);
                obj.Unk1 = ReadBool(buffer);
                obj.Unk2 = ReadUInt32(buffer);
                obj.Unk3 = ReadUInt32(buffer);
                obj.Unk4 = ReadUInt32(buffer);

                return obj;
            }
        }
    }
}