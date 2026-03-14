using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnlistedEntities.CustomContent
{
    /// <summary>
    /// Harmony patches for the save system to ensure equipable items are persisted.
    /// This approach converts equipped items into their "Item" pickup equivalents
    /// during serialization so they spawn as pickups on load.
    /// </summary>
    [HarmonyPatch(typeof(SaveLoadHandler))]
    public static class EquipableSavePatch
    {
        [HarmonyPatch("LoadSaveData")]
        [HarmonyPrefix]
        public static void LoadSaveDataPrefix(Save currentSave)
        {
            if (currentSave?.SerializedSave == null) return;

            var inventoryItems = currentSave.SerializedSave.InventoryItems;
            if (inventoryItems != null)
            {
                DbsContentApi.Modules.Logger.Log($"[SaveLoad] Found {inventoryItems.Length} inventory items in save.");
                foreach (var savedItem in inventoryItems)
                {
                    if (ItemDatabase.TryGetItemFromPersistentID(savedItem.GetPersistentID(), out var item))
                    {
                        DbsContentApi.Modules.Logger.Log($"[SaveLoad] Inventory Item: {item.displayName} (ID: {item.id}, PersistentID: {savedItem.persistentID})");
                    }
                    else
                    {
                        DbsContentApi.Modules.Logger.LogWarning($"[SaveLoad] Inventory Item in save not found in database! PersistentID: {savedItem.persistentID}");
                    }
                }
            }

            var surfaceItems = currentSave.SerializedSave.SurfaceItems;
            if (surfaceItems != null)
            {
                DbsContentApi.Modules.Logger.Log($"[SaveLoad] Found {surfaceItems.Length} surface items in save.");
                foreach (var savedItem in surfaceItems)
                {
                    if (ItemDatabase.TryGetItemFromPersistentID(savedItem.GetPersistentID(), out var item))
                    {
                        DbsContentApi.Modules.Logger.Log($"[SaveLoad] Surface Item: {item.displayName} (ID: {item.id}, PersistentID: {savedItem.persistentID}) at {savedItem.posX}, {savedItem.posY}, {savedItem.posZ}");
                    }
                    else
                    {
                        DbsContentApi.Modules.Logger.LogWarning($"[SaveLoad] Surface Item in save not found in database! PersistentID: {savedItem.persistentID}");
                    }
                }
            }
        }

        [HarmonyPatch("SerializeInventoryItems")]
        [HarmonyPostfix]
        public static void Postfix(ref SavedInventoryItem[] __result)
        {
            List<SavedInventoryItem> combinedItems = __result.ToList();
            int addedCount = 0;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (GlobalPlayerData.TryGetPlayerData(player, out var globalPlayerData))
                {
                    var equipableInventory = globalPlayerData.GetComponent<EquipableInventory>();
                    if (equipableInventory != null)
                    {
                        for (int i = 0; i < equipableInventory.equipableIDs.Length; i++)
                        {
                            byte itemID = equipableInventory.equipableIDs[i];
                            if (itemID == EquipableConfig.EMPTY_SLOT_ID) continue;

                            Item? item = GetItemForEquipable(itemID);
                            if (item != null)
                            {
                                DbsContentApi.Modules.Logger.Log($"[EquipableSavePatch] Adding item to save from equipable inventory: {item.persistentID}, {item.PersistentID})");
                                combinedItems.Add(new SavedInventoryItem(item));
                                addedCount++;
                            }
                        }
                    }
                }
            }

            if (addedCount > 0)
            {
                DbsContentApi.Modules.Logger.Log($"[EquipableSavePatch] Added {addedCount} equipped items to the save data.");
                __result = combinedItems.ToArray();
            }

            {
                {
                    // show the final list of inventory items we are saving
                    foreach (var item in __result)
                    {
                        DbsContentApi.Modules.Logger.Log($"[EquipableSavePatch] Saving item to save: {item.persistentID}, {item.GetPersistentID()})");
                    }
                }
            }
        }

        private static Item? GetItemForEquipable(byte itemID)
        {
            if (CustomItems.JumpingBootsItem != null && itemID == CustomItems.JumpingBootsItem.id)
                return CustomItems.JumpingBootsItem;

            if (CustomItems.CursedDoll != null && itemID == CustomItems.CursedDoll.id)
                return CustomItems.CursedDoll;

            if (CustomItems.AngelWingsItem != null && itemID == CustomItems.AngelWingsItem.id)
                return CustomItems.AngelWingsItem;

            if (CustomItems.GlowingVest != null && itemID == CustomItems.GlowingVest.id)
                return CustomItems.GlowingVest;

            return null;
        }
    }
}
