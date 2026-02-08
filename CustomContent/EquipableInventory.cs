using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class EquipableInventory : MonoBehaviourPun
{
    // Simple slot system: 0 = Boots, 1 = Charm (or generic)
    public byte[] equipableIDs = new byte[2] { 255, 255 };

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
        equipableIDs[slot] = itemID;
        UpdateVisuals();
    }

    public void ClearOnDeath()
    {
        if (photonView.IsMine)
        {
            SetEquipable(0, 255);
            SetEquipable(1, 255);
        }
    }

    private void UpdateVisuals()
    {
        // Low-tech: Find the Player object associated with this data
        // and toggle mesh renderers or change materials.
        // Example: if (equipableIDs[0] == BOOT_ID) ShowSpringBoots();
    }
}

[HarmonyPatch]
public class PlayerPatches
{
    // 1. Add our custom component to the PlayerData prefab/instance
    [HarmonyPatch(typeof(GlobalPlayerData), "Awake")]
    [HarmonyPostfix]
    static void AddEquipableComp(GlobalPlayerData __instance)
    {
        if (__instance.gameObject.GetComponent<EquipableInventory>() == null)
        {
            __instance.gameObject.AddComponent<EquipableInventory>();
        }
    }

    // 2. Clear items when the player dies
    // Note: Replace 'Player' and 'Die' with the actual game class/method found via exploration
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