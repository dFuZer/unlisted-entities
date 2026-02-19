using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnlistedEntities.CustomContent;

public class ExtendedPlayerData
{
    public byte[] equipableIDs = new byte[EquipableConfig.SLOT_COUNT];
    public Player player = null!;

    public ExtendedPlayerData(Player p)
    {
        player = p;
        UpdateData();
    }

    public void UpdateData()
    {
        if (player == null)
        {
            return;
        }

        if (!player.TryGetInventory(out var inventory))
        {
            return;
        }

        var equipables = inventory.gameObject.GetComponent<EquipableInventory>();
        if (equipables == null)
        {
            return;
        }
        equipableIDs = equipables.equipableIDs;
    }

    public bool PlayerHasEquipable(byte itemID)
    {
        return Array.IndexOf(equipableIDs, itemID) != -1;
    }
}

public static class PlayerCache
{
    private static ConditionalWeakTable<Player, ExtendedPlayerData> _table =
        new ConditionalWeakTable<Player, ExtendedPlayerData>();

    public static ExtendedPlayerData GetCache(Player player)
    {
        return _table.GetValue(player, p => new ExtendedPlayerData(p));
    }
}