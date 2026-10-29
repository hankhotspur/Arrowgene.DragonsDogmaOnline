using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Asset;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Arrowgene.Ddon.GameServer.Handler
{
    public class DailyMissionRewardReceiveHandler : GameRequestPacketHandler<C2SDailyMissionRewardReceiveReq, S2CDailyMissionRewardReceiveRes>
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(DailyMissionRewardReceiveHandler));

        // Values the client sends in C2SDailyMissionRewardReceiveReq.StorageType.
        private const byte StorageItemPost = 13;
        private const byte StorageItemBag = 19;
        private const byte StorageStorageBox = 20;

        public DailyMissionRewardReceiveHandler(DdonGameServer server) : base(server)
        {
        }

        public override S2CDailyMissionRewardReceiveRes Handle(GameClient client, C2SDailyMissionRewardReceiveReq request)
        {
            S2CDailyMissionRewardReceiveRes res = new S2CDailyMissionRewardReceiveRes();

            DailyMissionAsset asset = Server.AssetRepository.DailyMissionAsset;
            if (asset == null)
            {
                Logger.Error(client, "DailyMissionAsset is not loaded; nothing to claim.");
                return res;
            }

            if (request.StorageType != StorageItemPost && request.StorageType != StorageItemBag && request.StorageType != StorageStorageBox)
            {
                throw new ResponseErrorException(ErrorCode.ERROR_CODE_ITEM_INVALID_STORAGE_TYPE,
                    $"Unexpected storage type {request.StorageType} when claiming daily mission rewards");
            }

            var character = client.Character;
            Dictionary<uint, DailyMission> definitions = asset.DailyMissions.Concat(asset.SpecialRewards).ToDictionary(x => x.MissionId);
            Dictionary<uint, DailyMissionProgress> stored = Server.Database.SelectRewardMissionProgress(character.CommonId);

            // Only missions whose reward is waiting (State 2) can be claimed.
            List<DailyMission> claimedMissions = new();
            List<DailyMissionProgress> claimedRows = new();
            foreach (uint missionId in request.MissionIdList.Distinct())
            {
                if (!definitions.TryGetValue(missionId, out DailyMission mission)
                    || !stored.TryGetValue(missionId, out DailyMissionProgress row)
                    || row.State != DailyMissionState.RewardAvailable)
                {
                    Logger.Error(client, $"Daily mission {missionId} cannot be claimed. Skipping.");
                    continue;
                }

                claimedMissions.Add(mission);
                claimedRows.Add(row);
            }

            if (claimedMissions.Count == 0)
            {
                return res;
            }

            // Add up the rewards of everything being claimed.
            Dictionary<uint, uint> itemRewards = new();
            Dictionary<WalletType, uint> walletRewards = new();
            foreach (DailyMission mission in claimedMissions)
            {
                foreach (DailyMissionReward reward in mission.ItemList)
                {
                    itemRewards[reward.ItemId] = itemRewards.GetValueOrDefault(reward.ItemId) + reward.Amount;
                }

                foreach (DailyMissionReward reward in mission.WalletList)
                {
                    // For wallet rewards the JSON "item_id" is the wallet type (4 = silver tickets in the capture).
                    WalletType walletType = (WalletType)reward.ItemId;
                    if (!Enum.IsDefined(walletType))
                    {
                        Logger.Error(client, $"Daily mission {mission.MissionId} has an unknown wallet type {reward.ItemId}. Skipping that reward.");
                        continue;
                    }

                    walletRewards[walletType] = walletRewards.GetValueOrDefault(walletType) + reward.Amount;
                }
            }

            // Check that everything fits before changing anything, so a claim is all or nothing.
            Dictionary<StorageType, uint> slotsRequired = new();
            foreach (var (itemId, amount) in itemRewards)
            {
                StorageType target = GetDestinationStorage(request.StorageType, itemId);
                slotsRequired[target] = slotsRequired.GetValueOrDefault(target) + Server.ItemManager.PredictAddItemSlots(character, target, itemId, amount);
            }

            foreach (var (target, required) in slotsRequired)
            {
                if (character.Storage.GetStorage(target).EmptySlots() < required)
                {
                    throw new ResponseErrorException(request.StorageType == StorageItemBag
                        ? ErrorCode.ERROR_CODE_ITEM_BAG_CAPACITY_OVER
                        : ErrorCode.ERROR_CODE_ITEM_STORAGE_CAPACITY_OVER);
                }
            }

            S2CItemUpdateCharacterItemNtc itemNtc = new S2CItemUpdateCharacterItemNtc()
            {
                UpdateType = ItemNoticeType.RewardMission
            };

            Server.Database.ExecuteInTransaction(connection =>
            {
                foreach (var (itemId, amount) in itemRewards)
                {
                    List<CDataItemUpdateResult> results = AddRewardItem(client, request.StorageType, itemId, amount, connection);

                    uint added = (uint)results.Select(result => result.UpdateItemNum).Sum();
                    if (added != amount)
                    {
                        Logger.Error(client, $"Only {added} of {amount} of item {itemId} fit when claiming daily mission rewards.");
                    }

                    itemNtc.UpdateItemList.AddRange(results);
                }

                foreach (var (walletType, amount) in walletRewards)
                {
                    itemNtc.UpdateWalletList.Add(Server.WalletManager.AddToWallet(character, walletType, amount, 0, connection));
                }

                foreach (DailyMissionProgress row in claimedRows)
                {
                    row.State = DailyMissionState.RewardReceived;
                    if (!Server.Database.UpsertRewardMissionProgress(character.CommonId, row, connection))
                    {
                        Logger.Error(client, $"Failed to mark daily mission {row.MissionId} as received.");
                    }
                }
            });

            // The capture shows the notice first, then a response carrying the same updates.
            res.UpdateItemList = itemNtc.UpdateItemList;
            res.UpdateWalletList = itemNtc.UpdateWalletList;

            if (itemNtc.UpdateItemList.Count > 0 || itemNtc.UpdateWalletList.Count > 0)
            {
                client.Send(itemNtc);
            }

            return res;
        }

        /// <summary>
        /// Where an item ends up for the storage the player picked.
        /// Item bag: the bag that item belongs in. Storage box: the normal box.
        /// </summary>
        private StorageType GetDestinationStorage(byte requestStorageType, uint itemId)
        {
            return requestStorageType switch
            {
                StorageItemPost => StorageType.ItemPost,
                StorageItemBag => Server.AssetRepository.ClientItemInfos[itemId].StorageType,
                _ => StorageType.StorageBoxNormal
            };
        }

        private List<CDataItemUpdateResult> AddRewardItem(GameClient client, byte requestStorageType, uint itemId, uint amount, DbConnection connection)
        {
            return requestStorageType switch
            {
                StorageItemPost => Server.ItemManager.AddItem(Server, client.Character, StorageType.ItemPost, itemId, amount, connectionIn: connection),
                StorageItemBag => Server.ItemManager.AddItem(Server, client.Character, true, itemId, amount, connectionIn: connection),
                _ => Server.ItemManager.AddItem(Server, client.Character, false, itemId, amount, connectionIn: connection)
            };
        }
    }
}
