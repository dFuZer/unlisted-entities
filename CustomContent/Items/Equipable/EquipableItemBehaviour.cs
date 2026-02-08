using Photon.Pun;
using UnityEngine;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Base class for equipable items that provides common functionality
/// like checking for available slots and equipping logic.
/// Derived classes only need to override the virtual On* methods for custom behavior.
/// </summary>
public abstract class EquipableItemBehaviour : ItemInstanceBehaviour
{
    protected Player? player;

    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (isHeldByMe &&
            !Player.localPlayer.HasLockedInput() &&
            Player.localPlayer.input.clickWasPressed &&
            Player.localPlayer.TryGetInventory(out var inventory) &&
            inventory.TryGetSlot(Player.localPlayer.data.selectedItemSlot, out var slot))
        {
            TryEquip(inventory, slot);
        }
    }

    /// <summary>
    /// Attempts to equip the item into the player's equipable inventory.
    /// Handles all logic including duplicate checking, slot availability, and item consumption.
    /// </summary>
    private void TryEquip(PlayerInventory inventory, InventorySlot slot)
    {
        var equipableInventory = inventory.gameObject.GetComponent<EquipableInventory>();
        if (equipableInventory == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableItem] EquipableInventory component not found.");
            return;
        }

        // Check if this specific item is already equipped
        if (equipableInventory.HasEquipable(itemInstance.item.id))
        {
            OnAlreadyEquipped();
            return;
        }

        // Find the first available slot
        int availableSlot = equipableInventory.GetFirstAvailableSlot();
        if (availableSlot == -1)
        {
            OnNoAvailableSlots();
            return;
        }

        // Equip the item using the actual item ID from ItemDatabase
        equipableInventory.SetEquipable(availableSlot, itemInstance.item.id);
        OnEquipped(availableSlot);

        // Play visual effect
        Player.localPlayer.refs.emotes.DoBookEquipEffect(
            Player.localPlayer.refs.view.ViewID,
            itemInstance.item.id,
            base.transform.position,
            base.transform.rotation
        );

        // Consume the item from inventory
        slot.Clear();
    }

    /// <summary>
    /// Called when the item is successfully equipped.
    /// Override to add custom behavior on equip (e.g., play sounds, show messages).
    /// </summary>
    /// <param name="slotIndex">The slot index where the item was equipped.</param>
    protected virtual void OnEquipped(int slotIndex) { }

    /// <summary>
    /// Called when the item is already equipped and the player tries to equip it again.
    /// Override to add custom behavior (e.g., show a message to the player).
    /// </summary>
    protected virtual void OnAlreadyEquipped() { }

    /// <summary>
    /// Called when there are no available slots to equip the item.
    /// Override to add custom behavior (e.g., show a message to the player).
    /// </summary>
    protected virtual void OnNoAvailableSlots()
    {
        DbsContentApi.Modules.Logger.LogWarning("[EquipableItem] No available equipable slots.");
    }
}
