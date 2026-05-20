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
            Logger.Log("[DropEquipableInput] OnKeyDown triggered.");

            if (!GlobalPlayerData.TryGetPlayerData(player.refs.view.Owner, out var globalData))
            {
                Logger.LogError("[DropEquipableInput] Could not find GlobalPlayerData for player.");
                return;
            }
            Logger.Log("[DropEquipableInput] Found GlobalPlayerData.");

            var equipableInventory = globalData.GetComponent<EquipableInventory>();
            if (equipableInventory == null)
            {
                Logger.LogError("[DropEquipableInput] Could not find EquipableInventory on GlobalPlayerData.");
                return;
            }
            Logger.Log("[DropEquipableInput] Found EquipableInventory.");

            // Drop the first occupied slot
            for (int i = 0; i < EquipableConfig.SLOT_COUNT; i++)
            {
                byte itemID = equipableInventory.equipableIDs[i];
                Logger.Log($"[DropEquipableInput] Slot {i} has itemID: {itemID} (EMPTY = {EquipableConfig.EMPTY_SLOT_ID})");

                if (itemID != EquipableConfig.EMPTY_SLOT_ID)
                {
                    Item? item = Items.GetItemByID(itemID);
                    if (item != null)
                    {
                        Logger.Log($"[DropEquipableInput] Dropping item: {item.name} from slot {i}.");
                        var hip = player.GetRig(BodypartType.Hip);
                        player.RequestCreatePickup(
                            item,
                            new ItemInstanceData(System.Guid.NewGuid()),
                            hip.position + hip.transform.rotation * Vector3.forward * 1f,
                            UnityEngine.Quaternion.identity
                        );
                        equipableInventory.SetEquipable(i, EquipableConfig.EMPTY_SLOT_ID);
                        Logger.Log($"[DropEquipableInput] Successfully dropped item: {item.name} from slot {i}.");
                    }
                    else
                    {
                        Logger.LogError($"[DropEquipableInput] Could not find Item for itemID: {itemID}.");
                    }
                    break;
                }
            }
            Logger.Log("[DropEquipableInput] OnKeyDown finished — no occupied slots found.");
        }
        protected override void OnKeyUp(Player player) { }
        protected override void OnHeld(Player player) { }
    }
}