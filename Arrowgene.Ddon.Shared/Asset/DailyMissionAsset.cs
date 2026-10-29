using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using System;
using System.Collections.Generic;

namespace Arrowgene.Ddon.Shared.Asset
{
	/// TODO: Five weeks and custom periods
    public enum DailyMissionPeriod
    {
        Daily,
        Weekly
    }

    public enum DailyMissionListType
    {
        DailyMission,
        SpecialReward,
        TimeEvent
    }

    public class DailyMissionReward
    {
        public uint ItemId { get; set; }
        public uint Amount { get; set; }

        public CDataDailyMissionReward ToCData()
        {
            return new CDataDailyMissionReward()
            {
                Id = ItemId,
                Value = Amount
            };
        }
    }

    /// Unknown reward list. Fiddling with values revealed nothing so far.
    public class DailyMissionUnk1
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public ushort Unk3 { get; set; }
 
        public CDataDailyMissionUnk1 ToCData()
        {
            return new CDataDailyMissionUnk1()
            {
                Unk0 = Unk0,
                Unk1 = Unk1,
                Unk2 = Unk2,
                Unk3 = Unk3
            };
        }
    }

    /// Reward list for golden gemstones (code RWM001) sent by mail
    public class DailyMissionUnk2
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; } // Amount
        public uint Unk3 { get; set; }
 
        public CDataDailyMissionUnk2 ToCData()
        {
            return new CDataDailyMissionUnk2()
            {
                Unk0 = Unk0,
                Unk1 = Unk1,
                Unk2 = Unk2,
                Unk3 = Unk3
            };
        }
    }

    /// Unknown reward list that disconnects for any value (wrong fields?)
    public class DailyMissionUnk3
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
 
        public CDataDailyMissionUnk3 ToCData()
        {
            return new CDataDailyMissionUnk3()
            {
                Unk0 = Unk0,
                Unk1 = Unk1,
                Unk2 = Unk2,
                Unk3 = Unk3,
                Unk4 = Unk4
            };
        }
    }

    /// Reward list for Craft Master pawn sent by mail
    public class DailyMissionUnk4
    {
        public ulong Unk0 { get; set; }
        public bool Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public string Unk3 { get; set; } = string.Empty;
 
        public CDataDailyMissionUnk4 ToCData()
        {
            return new CDataDailyMissionUnk4()
            {
                Unk0 = Unk0,
                Unk1 = Unk1,
                Unk2 = Unk2,
                Unk3 = Unk3
            };
        }
    }

    public class DailyMission
    {
        public DailyMission()
        {
            ItemList = new List<DailyMissionReward>();
            WalletList = new List<DailyMissionReward>();
            Title = string.Empty;
            IconUrl = string.Empty;
            ImageUrl = string.Empty;
            RewardCode = string.Empty;
            UnkList1 = new List<DailyMissionUnk1>();
            UnkList2 = new List<DailyMissionUnk2>();
            UnkList3 = new List<DailyMissionUnk3>();
            UnkList4 = new List<DailyMissionUnk4>();
        }

        public uint MissionId { get; set; }
        public uint Objective { get; set; }
        public uint Goal { get; set; }
        public uint SortNo { get; set; }
        public uint Unk0 { get; set; } = 999;
        public DailyMissionPeriod Period { get; set; }
        public DailyMissionListType ListType { get; set; }
        public List<DailyMissionReward> ItemList { get; set; }
        public List<DailyMissionReward> WalletList { get; set; }
        public string RewardCode { get; set; }
        public List<DailyMissionUnk1> UnkList1 { get; set; }
        public List<DailyMissionUnk2> UnkList2 { get; set; }
        public List<DailyMissionUnk3> UnkList3 { get; set; }
        public List<DailyMissionUnk4> UnkList4 { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }

        public CDataDailyMission ToCData(DateTimeOffset begin, DateTimeOffset end, uint progress, DailyMissionState state)
        {
            var obj = new CDataDailyMission()
            {
                MissionId = MissionId,
                Objective = Objective,
                Goal = Goal,
                Title = Title,
                IconUrl = IconUrl,
                ImageUrl = ImageUrl,
                Progress = progress,
                State = (byte)state,
                Begin = begin,
                End = end,
                SortNo = SortNo,
                MissionDetail = new CDataDailyMissionDetail()
                {
                    MissionId = MissionId,
                    Unk0 = Unk0,
                    Begin = DateTimeOffset.FromUnixTimeSeconds(0),
                    End = DateTimeOffset.FromUnixTimeSeconds(0),
                    RewardCode = RewardCode
                }
            };

            foreach (var reward in ItemList)
            {
                obj.MissionDetail.ItemReward.Add(reward.ToCData());
            }

            foreach (var reward in WalletList)
            {
                obj.MissionDetail.WalletReward.Add(reward.ToCData());
            }

            foreach (var entry in UnkList1)
            {
                obj.MissionDetail.UnkList1.Add(entry.ToCData());
            }
 
            foreach (var entry in UnkList2)
            {
                obj.MissionDetail.UnkList2.Add(entry.ToCData());
            }
 
            foreach (var entry in UnkList3)
            {
                obj.MissionDetail.UnkList3.Add(entry.ToCData());
            }
 
            foreach (var entry in UnkList4)
            {
                obj.MissionDetail.UnkList4.Add(entry.ToCData());
            }

            return obj;
        }
    }

    public class DailyMissionAsset
    {
        public DailyMissionAsset()
        {
            DailyMissions = new List<DailyMission>();
            SpecialRewards = new List<DailyMission>();
            TimeEvents = new List<DailyMission>();
        }

        public List<DailyMission> DailyMissions { get; set; }
        public List<DailyMission> SpecialRewards { get; set; }
        public List<DailyMission> TimeEvents { get; set; }

        public uint ResetHour { get; set; } = 5;
        public DayOfWeek WeeklyResetDay { get; set; } = DayOfWeek.Monday;

        public (DateTimeOffset Begin, DateTimeOffset End) GetPeriod(DailyMissionPeriod period, DateTimeOffset now)
        {
            var resetToday = new DateTimeOffset(
                now.Year, now.Month, now.Day,
                (int)ResetHour, 0, 0, now.Offset);

            if (period == DailyMissionPeriod.Daily)
            {
                var begin = (now < resetToday) ? resetToday.AddDays(-1) : resetToday;
                return (begin, begin.AddDays(1));
            }

            int daysSinceReset = ((int)now.DayOfWeek - (int)WeeklyResetDay + 7) % 7;
            var weekBegin = resetToday.AddDays(-daysSinceReset);
            if (now < weekBegin)
            {
                weekBegin = weekBegin.AddDays(-7);
            }

            return (weekBegin, weekBegin.AddDays(7));
        }
    }
}
