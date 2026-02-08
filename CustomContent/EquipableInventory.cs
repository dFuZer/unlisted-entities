using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages a player's equipable items inventory. Each player has a fixed number of equipable slots
/// that persist across the network via Photon RPC calls. Equipable items are identified by their item ID.
/// </summary>
public class EquipableInventory : MonoBehaviourPun
{
    // Simple slot system: 0 = Boots, 1 = Charm (or generic)
    public byte[] equipableIDs = new byte[EquipableConfig.SLOT_COUNT];

    private void Awake()
    {
        // Initialize all slots to empty
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            equipableIDs[i] = EquipableConfig.EMPTY_SLOT_ID;
        }
    }

    /// <summary>
    /// Sets an equipable item in the specified slot and synchronizes across the network.
    /// </summary>
    /// <param name="slot">The slot index to set.</param>
    /// <param name="itemID">The item ID to place in the slot, or EMPTY_SLOT_ID to clear.</param>
    public void SetEquipable(int slot, byte itemID)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_SyncEquipable", RpcTarget.AllBuffered, slot, itemID);
        }
    }

    [PunRPC]
    public void RPC_SyncEquipable(int slot, byte itemID)
    {
        if (slot >= 0 && slot < equipableIDs.Length)
        {
            equipableIDs[slot] = itemID;
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Clears all equipable slots when the player dies.
    /// </summary>
    public void ClearOnDeath()
    {
        if (photonView.IsMine)
        {
            for (int i = 0; i < EquipableConfig.SLOT_COUNT; i++)
            {
                SetEquipable(i, EquipableConfig.EMPTY_SLOT_ID);
            }
        }
    }

    /// <summary>
    /// Finds the first available (empty) equipable slot.
    /// </summary>
    /// <returns>The index of the first available slot, or -1 if all slots are occupied.</returns>
    public int GetFirstAvailableSlot()
    {
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            if (equipableIDs[i] == EquipableConfig.EMPTY_SLOT_ID)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Checks if a specific item is currently equipped in any slot.
    /// </summary>
    /// <param name="itemID">The item ID to check for.</param>
    /// <returns>True if the item is equipped, false otherwise.</returns>
    public bool HasEquipable(byte itemID)
    {
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            if (equipableIDs[i] == itemID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the slot index where a specific item is equipped.
    /// </summary>
    /// <param name="itemID">The item ID to find.</param>
    /// <returns>The slot index, or -1 if not found.</returns>
    public int GetSlotForItem(byte itemID)
    {
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            if (equipableIDs[i] == itemID)
            {
                return i;
            }
        }
        return -1;
    }

    private void UpdateVisuals()
    {
        // Low-tech: Find the Player object associated with this data
        // and toggle mesh renderers or change materials.
        // Example: if (equipableIDs[0] == BOOT_ID) ShowSpringBoots();
    }
}

/// <summary>
/// Harmony patches for integrating the EquipableInventory system with the game's player lifecycle.
/// </summary>
[HarmonyPatch]
public class PlayerPatches
{
    /// <summary>
    /// Adds the EquipableInventory component to GlobalPlayerData when it awakens.
    /// Ensures each player has an equipable inventory attached.
    /// </summary>
    [HarmonyPatch(typeof(GlobalPlayerData), "Awake")]
    [HarmonyPostfix]
    static void AddEquipableComp(GlobalPlayerData __instance)
    {
        if (__instance.gameObject.GetComponent<EquipableInventory>() == null)
        {
            __instance.gameObject.AddComponent<EquipableInventory>();
        }
    }

    /// <summary>
    /// Clears all equipable items when the player dies.
    /// </summary>
    [HarmonyPatch(typeof(Player), "Die")]
    [HarmonyPostfix]
    static void OnPlayerDie(Player __instance)
    {
        // Find the PlayerData for this player
        if (GlobalPlayerData.TryGetPlayerData(__instance.refs.view.Owner, out var data))
        {
            var inv = data.GetComponent<EquipableInventory>();
            if (inv != null) inv.ClearOnDeath();
        }
    }
}