using Arrowgene.Buffers;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Entity.Structure
{
    public class CDataDailyMissionUnk1
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public ushort Unk3 { get; set; }

        public class Serializer : EntitySerializer<CDataDailyMissionUnk1>
        {
            public override void Write(IBuffer buffer, CDataDailyMissionUnk1 obj)
            {
                WriteUInt64(buffer, obj.Unk0);
                WriteBool(buffer, obj.Unk1);
                WriteUInt32(buffer, obj.Unk2);
                WriteUInt16(buffer, obj.Unk3);
            }

            public override CDataDailyMissionUnk1 Read(IBuffer buffer)
            {
                CDataDailyMissionUnk1 obj = new CDataDailyMissionUnk1();

                obj.Unk0 = ReadUInt64(buffer);
                obj.Unk1 = ReadBool(buffer);
                obj.Unk2 = ReadUInt32(buffer);
                obj.Unk3 = ReadUInt16(buffer);

                return obj;
            }
        }
    }
}