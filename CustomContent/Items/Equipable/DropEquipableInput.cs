using DbsContentApi.Modules;
using Zorro.Settings;
using UnityEngine;
namespace UnlistedEntities.CustomContent
{
    public class DropEquipableInput : BaseCWInput, IExposedSetting
    {
        protected override KeyCode GetDefaultKey() => KeyCode.G;
        public string GetDisplayName() => "Drop Equipable Item";
        public SettingCategory GetSettingCategory() => (SettingCategory)2;
        protected override void OnKeyDown(Player player)
        {
            DbsContentApi.Modules.Logger.Log("[DropEquipableInput] OnKeyDown triggered.");

            if (!GlobalPlayerData.TryGetPlayerData(player.refs.view.Owner, out var globalData))
            {
                DbsContentApi.Modules.Logger.LogError("[DropEquipableInput] Could not find GlobalPlayerData for player.");
                return;
            }
            DbsContentApi.Modules.Logger.Log("[DropEquipableInput] Found GlobalPlayerData.");

            var equipableInventory = globalData.GetComponent<EquipableInventory>();
            if (equipableInventory == null)
            {
                DbsContentApi.Modules.Logger.LogError("[DropEquipableInput] Could not find EquipableInventory on GlobalPlayerData.");
                return;
            }
            DbsContentApi.Modules.Logger.Log("[DropEquipableInput] Found EquipableInventory.");

            // Drop the first occupied slot
            for (int i = 0; i < EquipableConfig.SLOT_COUNT; i++)
            {
                byte itemID = equipableInventory.equipableIDs[i];
                DbsContentApi.Modules.Logger.Log($"[DropEquipableInput] Slot {i} has itemID: {itemID} (EMPTY = {EquipableConfig.EMPTY_SLOT_ID})");

                if (itemID != EquipableConfig.EMPTY_SLOT_ID)
                {
                    Item? item = Items.GetItemByID(itemID);
                    if (item != null)
                    {
                        DbsContentApi.Modules.Logger.Log($"[DropEquipableInput] Dropping item: {item.name} from slot {i}.");
                        player.RequestCreatePickup(
                            item,
                            new ItemInstanceData(System.Guid.NewGuid()),
                            player.Center(),
                            UnityEngine.Quaternion.identity
                        );
                        equipableInventory.SetEquipable(i, EquipableConfig.EMPTY_SLOT_ID);
                        DbsContentApi.Modules.Logger.Log($"[DropEquipableInput] Successfully dropped item: {item.name} from slot {i}.");
                    }
                    else
                    {
                        DbsContentApi.Modules.Logger.LogError($"[DropEquipableInput] Could not find Item for itemID: {itemID}.");
                    }
                    break;
                }
            }
            DbsContentApi.Modules.Logger.Log("[DropEquipableInput] OnKeyDown finished — no occupied slots found.");
        }
        protected override void OnKeyUp(Player player) { }
        protected override void OnHeld(Player player) { }
    }
}