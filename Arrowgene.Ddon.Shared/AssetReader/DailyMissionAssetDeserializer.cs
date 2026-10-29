using Arrowgene.Ddon.Shared.Asset;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Arrowgene.Ddon.Shared.AssetReader
{
    public class DailyMissionAssetDeserializer : IAssetDeserializer<DailyMissionAsset>
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(DailyMissionAssetDeserializer));

        private static readonly HashSet<string> RootKeys = new()
        {
            "comment", "reset_hour", "weekly_reset_day", "daily_missions", "special_reward", "time_event"
        };

        private static readonly HashSet<string> MissionKeys = new()
        {
            "comment", "mission_id", "title", "reset_period", "objective", "goal", "sort_no",
            "icon_url", "image_url", "reward_code", "item_reward", "wallet_reward",
            "unk_list_1", "unk_list_2", "unk_list_3", "unk_list_4"
        };

        private static readonly HashSet<string> RewardKeys = new()
        {
            "comment", "item_id", "amount"
        };

        // Entries of the four unknown lists. They all start with unk0, unk1, unk2;
        // list 3 has a further unk4.
        private static readonly HashSet<string> UnkEntryKeys = new()
        {
            "comment", "unk0", "unk1", "unk2", "unk3"
        };
 
        private static readonly HashSet<string> UnkEntryKeysWithUnk4 = new()
        {
            "comment", "unk0", "unk1", "unk2", "unk3", "unk4"
        };

        public DailyMissionAsset ReadPath(string path)
        {
            Logger.Info($"Reading {path}");

            DailyMissionAsset asset = new DailyMissionAsset();

            string json = Util.ReadAllText(path);
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                Logger.Error($"{path}: expected a JSON object at the top level.");
                return asset;
            }

            WarnUnknownKeys(root, RootKeys, "top level");

            if (root.TryGetProperty("reset_hour", out JsonElement jResetHour))
            {
                if (jResetHour.ValueKind == JsonValueKind.Number && jResetHour.TryGetUInt32(out uint resetHour) && resetHour < 24)
                {
                    asset.ResetHour = resetHour;
                }
                else
                {
                    Logger.Error($"Invalid reset_hour (expected 0-23). Defaulting to {asset.ResetHour}.");
                }
            }

            if (root.TryGetProperty("weekly_reset_day", out JsonElement jResetDay))
            {
                string dayText = jResetDay.ValueKind == JsonValueKind.String ? jResetDay.GetString() : null;
                if (dayText != null && Enum.TryParse(dayText, true, out DayOfWeek resetDay) && Enum.IsDefined(resetDay))
                {
                    asset.WeeklyResetDay = resetDay;
                }
                else
                {
                    Logger.Error($"Invalid weekly_reset_day. Defaulting to {asset.WeeklyResetDay}.");
                }
            }

            ParseMissionList(asset.DailyMissions, root, "daily_missions", DailyMissionListType.DailyMission);
            ParseMissionList(asset.SpecialRewards, root, "special_reward", DailyMissionListType.SpecialReward);
            ParseMissionList(asset.TimeEvents, root, "time_event", DailyMissionListType.TimeEvent);

            HashSet<uint> seenIds = new HashSet<uint>();
            RemoveDuplicateIds(asset.DailyMissions, seenIds);
            RemoveDuplicateIds(asset.SpecialRewards, seenIds);
            RemoveDuplicateIds(asset.TimeEvents, seenIds);

            Logger.Info($"Loaded {asset.DailyMissions.Count} missions, {asset.SpecialRewards.Count} special rewards, {asset.TimeEvents.Count} time events.");

            return asset;
        }

        private void ParseMissionList(List<DailyMission> target, JsonElement root, string key, DailyMissionListType listType)
        {
            if (!root.TryGetProperty(key, out JsonElement jMissions))
            {
                // Each list is optional; an absent list is simply empty.
                return;
            }

            if (jMissions.ValueKind != JsonValueKind.Array)
            {
                Logger.Error($"'{key}' must be an array. Ignoring it.");
                return;
            }

            int index = 0;
            foreach (JsonElement jMission in jMissions.EnumerateArray())
            {
                DailyMission mission = ParseMission(jMission, listType, $"{key}[{index}]");
                index++;

                if (mission != null)
                {
                    target.Add(mission);
                }
            }
        }

        private DailyMission ParseMission(JsonElement jMission, DailyMissionListType listType, string context)
        {
            if (jMission.ValueKind != JsonValueKind.Object)
            {
                Logger.Error($"{context} is not an object. Skipping mission.");
                return null;
            }

            WarnUnknownKeys(jMission, MissionKeys, context);

            if (!TryReadUInt(jMission, "mission_id", context, out uint missionId))
            {
                return null;
            }
            context = $"{context} (mission_id {missionId})";

            if (!TryReadUInt(jMission, "objective", context, out uint objective))
            {
                return null;
            }

            if (!TryReadUInt(jMission, "goal", context, out uint goal))
            {
                return null;
            }

            if (!TryReadUInt(jMission, "sort_no", context, out uint sortNo))
            {
                return null;
            }

            if (!TryReadString(jMission, "title", context, out string title))
            {
                return null;
            }

            if (!TryReadString(jMission, "reset_period", context, out string periodText))
            {
                return null;
            }

            if (!Enum.TryParse(periodText, true, out DailyMissionPeriod period) || !Enum.IsDefined(period))
            {
                Logger.Error($"{context}: invalid reset_period '{periodText}'. Skipping mission.");
                return null;
            }

            DailyMission mission = new DailyMission()
            {
                ListType = listType,
                MissionId = missionId,
                Objective = objective,
                Goal = goal,
                SortNo = sortNo,
                Title = title,
                Period = period,
                IconUrl = ReadOptionalString(jMission, "icon_url"),
                ImageUrl = ReadOptionalString(jMission, "image_url"),
                RewardCode = ReadOptionalString(jMission, "reward_code"),
            };

            // A reward list that can't be read skips the whole mission, so a
            // half-defined mission is never sent to the client.
            if (!TryParseRewards(jMission, "item_reward", context, mission.ItemList))
            {
                return null;
            }

            if (!TryParseRewards(jMission, "wallet_reward", context, mission.WalletList))
            {
                return null;
            }

            if (!TryParseUnkList(jMission, "unk_list_1", context, mission.UnkList1, UnkEntryKeys, ReadUnk1Entry)
                || !TryParseUnkList(jMission, "unk_list_2", context, mission.UnkList2, UnkEntryKeys, ReadUnk2Entry)
                || !TryParseUnkList(jMission, "unk_list_3", context, mission.UnkList3, UnkEntryKeysWithUnk4, ReadUnk3Entry)
                || !TryParseUnkList(jMission, "unk_list_4", context, mission.UnkList4, UnkEntryKeys, ReadUnk4Entry))
            {
                return null;
            }

            return mission;
        }

        private static bool TryParseRewards(JsonElement jMission, string key, string context, List<DailyMissionReward> target)
        {
            if (!jMission.TryGetProperty(key, out JsonElement jRewards))
            {
                return true;
            }

            if (jRewards.ValueKind != JsonValueKind.Array)
            {
                Logger.Error($"{context}: '{key}' must be an array. Skipping mission.");
                return false;
            }

            int index = 0;
            foreach (JsonElement jReward in jRewards.EnumerateArray())
            {
                string rewardContext = $"{context} {key}[{index}]";
                index++;

                if (jReward.ValueKind != JsonValueKind.Object)
                {
                    Logger.Error($"{rewardContext} is not an object. Skipping mission.");
                    return false;
                }

                WarnUnknownKeys(jReward, RewardKeys, rewardContext);

                if (!TryReadUInt(jReward, "item_id", rewardContext, out uint itemId))
                {
                    return false;
                }

                if (!TryReadUInt(jReward, "amount", rewardContext, out uint amount))
                {
                    return false;
                }

                target.Add(new DailyMissionReward()
                {
                    ItemId = itemId,
                    Amount = amount
                });
            }

            return true;
        }

        private static bool TryParseUnkList<T>(
            JsonElement jMission, string key, string context, List<T> target,
            HashSet<string> knownKeys, Func<JsonElement, string, T> readEntry) where T : class
        {
            if (!jMission.TryGetProperty(key, out JsonElement jEntries))
            {
                return true;
            }
 
            if (jEntries.ValueKind != JsonValueKind.Array)
            {
                Logger.Error($"{context}: '{key}' must be an array. Skipping mission.");
                return false;
            }
 
            int index = 0;
            foreach (JsonElement jEntry in jEntries.EnumerateArray())
            {
                string entryContext = $"{context} {key}[{index}]";
                index++;
 
                if (jEntry.ValueKind != JsonValueKind.Object)
                {
                    Logger.Error($"{entryContext} is not an object. Skipping mission.");
                    return false;
                }
 
                WarnUnknownKeys(jEntry, knownKeys, entryContext);
 
                T entry = readEntry(jEntry, entryContext);
                if (entry == null)
                {
                    return false;
                }
 
                target.Add(entry);
            }
 
            return true;
        }
 
        private static DailyMissionUnk1 ReadUnk1Entry(JsonElement jEntry, string context)
        {
            if (!TryReadULong(jEntry, "unk0", context, out ulong unk0)) return null;
            if (!TryReadBool(jEntry, "unk1", context, out bool unk1)) return null;
            if (!TryReadUInt(jEntry, "unk2", context, out uint unk2)) return null;
            if (!TryReadUShort(jEntry, "unk3", context, out ushort unk3)) return null;
 
            return new DailyMissionUnk1() { Unk0 = unk0, Unk1 = unk1, Unk2 = unk2, Unk3 = unk3 };
        }
 
        private static DailyMissionUnk2 ReadUnk2Entry(JsonElement jEntry, string context)
        {
            if (!TryReadULong(jEntry, "unk0", context, out ulong unk0)) return null;
            if (!TryReadBool(jEntry, "unk1", context, out bool unk1)) return null;
            if (!TryReadUInt(jEntry, "unk2", context, out uint unk2)) return null;
            if (!TryReadUInt(jEntry, "unk3", context, out uint unk3)) return null;
 
            return new DailyMissionUnk2() { Unk0 = unk0, Unk1 = unk1, Unk2 = unk2, Unk3 = unk3 };
        }
 
        private static DailyMissionUnk3 ReadUnk3Entry(JsonElement jEntry, string context)
        {
            if (!TryReadULong(jEntry, "unk0", context, out ulong unk0)) return null;
            if (!TryReadBool(jEntry, "unk1", context, out bool unk1)) return null;
            if (!TryReadUInt(jEntry, "unk2", context, out uint unk2)) return null;
            if (!TryReadUInt(jEntry, "unk3", context, out uint unk3)) return null;
            if (!TryReadUInt(jEntry, "unk4", context, out uint unk4)) return null;
 
            return new DailyMissionUnk3() { Unk0 = unk0, Unk1 = unk1, Unk2 = unk2, Unk3 = unk3, Unk4 = unk4 };
        }
 
        private static DailyMissionUnk4 ReadUnk4Entry(JsonElement jEntry, string context)
        {
            if (!TryReadULong(jEntry, "unk0", context, out ulong unk0)) return null;
            if (!TryReadBool(jEntry, "unk1", context, out bool unk1)) return null;
            if (!TryReadUInt(jEntry, "unk2", context, out uint unk2)) return null;
            if (!TryReadString(jEntry, "unk3", context, out string unk3)) return null;
 
            return new DailyMissionUnk4() { Unk0 = unk0, Unk1 = unk1, Unk2 = unk2, Unk3 = unk3 };
        }

        private static bool TryReadULong(JsonElement parent, string key, string context, out ulong value)
        {
            value = 0;
            if (!parent.TryGetProperty(key, out JsonElement jValue)
                || jValue.ValueKind != JsonValueKind.Number
                || !jValue.TryGetUInt64(out value))
            {
                Logger.Error($"{context}: '{key}' is missing or is not a whole number. Skipping mission.");
                return false;
            }
 
            return true;
        }

        private static bool TryReadBool(JsonElement parent, string key, string context, out bool value)
        {
            value = false;
            if (!parent.TryGetProperty(key, out JsonElement jValue)
                || (jValue.ValueKind != JsonValueKind.True && jValue.ValueKind != JsonValueKind.False))
            {
                Logger.Error($"{context}: '{key}' is missing or is not true/false. Skipping mission.");
                return false;
            }
 
            value = jValue.GetBoolean();
            return true;
        }

        private static bool TryReadUShort(JsonElement parent, string key, string context, out ushort value)
        {
            value = 0;
            if (!parent.TryGetProperty(key, out JsonElement jValue)
                || jValue.ValueKind != JsonValueKind.Number
                || !jValue.TryGetUInt16(out value))
            {
                Logger.Error($"{context}: '{key}' is missing or is not a whole number from 0 to 65535. Skipping mission.");
                return false;
            }
 
            return true;
        }

        private static bool TryReadUInt(JsonElement parent, string key, string context, out uint value)
        {
            value = 0;
            if (!parent.TryGetProperty(key, out JsonElement jValue)
                || jValue.ValueKind != JsonValueKind.Number
                || !jValue.TryGetUInt32(out value))
            {
                Logger.Error($"{context}: '{key}' is missing or is not a whole number. Skipping mission.");
                return false;
            }

            return true;
        }

        private static bool TryReadString(JsonElement parent, string key, string context, out string value)
        {
            value = null;
            if (!parent.TryGetProperty(key, out JsonElement jValue) || jValue.ValueKind != JsonValueKind.String)
            {
                Logger.Error($"{context}: '{key}' is missing or is not a string. Skipping mission.");
                return false;
            }

            value = jValue.GetString();
            return true;
        }

        private static string ReadOptionalString(JsonElement parent, string key)
        {
            if (parent.TryGetProperty(key, out JsonElement jValue) && jValue.ValueKind == JsonValueKind.String)
            {
                return jValue.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static void WarnUnknownKeys(JsonElement element, HashSet<string> knownKeys, string context)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!knownKeys.Contains(property.Name))
                {
                    Logger.Error($"{context}: unknown key '{property.Name}' is ignored.");
                }
            }
        }

        private static void RemoveDuplicateIds(List<DailyMission> missions, HashSet<uint> seenIds)
        {
            missions.RemoveAll(mission =>
            {
                if (seenIds.Add(mission.MissionId))
                {
                    return false;
                }

                Logger.Error($"Duplicate mission_id {mission.MissionId} in {mission.ListType}. Skipping the later entry.");
                return true;
            });
        }
    }
}
