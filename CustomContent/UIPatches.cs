using System.Linq;
using HarmonyLib;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using UnlistedEntities.CustomContent;

/// <summary>
/// Configuration constants for the equipable system.
/// </summary>
public static class EquipableConfig
{
    /// <summary>
    /// Byte value representing an empty equipable slot.
    /// </summary>
    public const byte EMPTY_SLOT_ID = 255;

    /// <summary>
    /// Number of equipable slots available to the player.
    /// </summary>
    public const int SLOT_COUNT = 2;

    /// <summary>
    /// Canvas group alpha value when slot is empty.
    /// </summary>
    public const float ALPHA_EMPTY = 0.4f;

    /// <summary>
    /// Canvas group alpha value when slot contains an item.
    /// </summary>
    public const float ALPHA_FILLED = 0.7f;
}

/// <summary>
/// Harmony patch for UserInterface that implements a self-healing initialization strategy
/// for equipable UI slots. Monitors for missing UI elements and recreates them as needed.
/// </summary>
[HarmonyPatch(typeof(UserInterface))]
public class UserInterfacePatch
{
    private static HotbarUIExtension? hotbarUIExtension;
    private static GameObject[] slots = new GameObject[EquipableConfig.SLOT_COUNT] { null!, null! };

    [HarmonyPatch("LateUpdate"), HarmonyPostfix]
    static void UserInterfaceAwakePostfix(UserInterface __instance)
    {
        if (Player.localPlayer != null && slots.Any(slot => slot == null))
        {
            InitUI(__instance);
        }

        if (hotbarUIExtension != null && Player.localPlayer != null)
        {
            hotbarUIExtension.Set(Player.localPlayer);
        }
    }

    static void InitUI(UserInterface ui)
    {
        var hotbarGameObjectTransform = ui.gameObject.transform.Find("Pivot/Others/Hotbar");
        if (hotbarGameObjectTransform == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableUI] Failed to initialize: Hotbar GameObject not found in UI hierarchy.");
            return;
        }

        HotbarUIExtension hotbarUIExtension = hotbarGameObjectTransform.gameObject.AddComponent<HotbarUIExtension>();
        UserInterfacePatch.hotbarUIExtension = hotbarUIExtension;
        hotbarUIExtension.slots = new EquipableSlotUI[EquipableConfig.SLOT_COUNT];

        var itemsGameObject = hotbarGameObjectTransform.Find("Items");
        if (itemsGameObject == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableUI] Failed to initialize: Items GameObject not found under Hotbar.");
            return;
        }

        var slot1 = itemsGameObject.transform.Find("HotbarSlot");
        if (slot1 == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableUI] Failed to initialize: HotbarSlot template not found.");
            return;
        }
        for (int i = EquipableConfig.SLOT_COUNT - 1; i >= 0; i--)
        {
            if (itemsGameObject.transform.Find($"EquipableSlot{i + 1}") != null)
            {
                DbsContentApi.Modules.Logger.LogError($"[EquipableUI] EquipableSlot{i + 1} already exists, skipping creation.");
                continue;
            }

            GameObject clone = UnityEngine.Object.Instantiate(slot1.gameObject);
            HotbarSlotUI hotbarSlotUI = clone.GetComponent<HotbarSlotUI>();
            EquipableSlotUI equipableSlotUI = clone.AddComponent<EquipableSlotUI>();

            // Transfer icon reference from HotbarSlotUI before destroying it
            equipableSlotUI.m_icon = hotbarSlotUI.m_icon;
            equipableSlotUI.m_unknownIcon = hotbarSlotUI.m_unkownIcon;
            UnityEngine.Object.DestroyImmediate(hotbarSlotUI);

            clone.transform.SetParent(itemsGameObject);
            clone.name = $"EquipableSlot{i + 1}";
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localScale = Vector3.one;
            clone.transform.rotation = Quaternion.identity;
            clone.transform.localRotation = Quaternion.identity;

            Graphic proceduralImage = clone.GetComponent<ProceduralImage>();
            if (proceduralImage != null)
            {
                proceduralImage.color = new Color(1f, 0f, 0f, 1f);
            }

            clone.transform.SetAsFirstSibling();

            var headerTmp = clone.transform.Find("Header")?.GetComponent<TextMeshProUGUI>();
            if (headerTmp == null)
            {
                DbsContentApi.Modules.Logger.LogError($"[EquipableUI] Header TextMeshProUGUI not found for slot {i + 1}.");
                continue;
            }

            headerTmp.text = $"Eq {i + 1}";
            slots[i] = clone;
            hotbarUIExtension.slots[i] = equipableSlotUI;
        }

        DbsContentApi.Modules.Logger.Log($"[EquipableUI] Initialized {EquipableConfig.SLOT_COUNT} equipable slots successfully.");
    }
}

/// <summary>
/// Manages the array of equipable slots and orchestrates updates when the player's
/// equipable inventory changes. Implements change detection to minimize UI updates.
/// </summary>
public class HotbarUIExtension : MonoBehaviour
{
    public EquipableSlotUI[] slots = null!;
    private byte[]? m_previousEquipableIDs;

    /// <summary>
    /// Updates all equipable slots based on the player's current equipable inventory.
    /// Only updates slots whose equipable ID has changed since the last update.
    /// </summary>
    /// <param name="player">The player whose equipable inventory should be displayed.</param>
    public void Set(Player player)
    {
        if (!player.TryGetInventory(out var inventory))
        {
            return;
        }

        var equipables = inventory.gameObject.GetComponent<EquipableInventory>();
        if (equipables == null)
        {
            return;
        }

        // Initialize previous state tracking on first run
        if (m_previousEquipableIDs == null)
        {
            m_previousEquipableIDs = new byte[EquipableConfig.SLOT_COUNT];
            for (int i = 0; i < m_previousEquipableIDs.Length; i++)
            {
                m_previousEquipableIDs[i] = EquipableConfig.EMPTY_SLOT_ID;
            }
        }

        // Only update slots that have changed
        for (int i = 0; i < slots.Length && i < equipables.equipableIDs.Length; i++)
        {
            byte currentID = equipables.equipableIDs[i];
            if (currentID != m_previousEquipableIDs[i])
            {
                slots[i].SetData(currentID);
                m_previousEquipableIDs[i] = currentID;
            }
        }
    }
}

/// <summary>
/// Renders an individual equipable slot in the UI. Displays the item's icon and
/// adjusts visual state based on whether the slot is occupied or empty.
/// Integrates with ItemDatabase to look up item icons by ID.
/// </summary>
public class EquipableSlotUI : MonoBehaviour
{
    public Image m_icon = null!;
    public Sprite m_unknownIcon = null!;
    public CanvasGroup canvasGroup = null!;

    public void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Updates the slot display based on the equipable item ID.
    /// Looks up the item in ItemDatabase and displays its icon.
    /// </summary>
    /// <param name="equipableID">The item ID to display, or EMPTY_SLOT_ID for an empty slot.</param>
    public void SetData(byte equipableID)
    {
        if (canvasGroup == null || m_icon == null)
        {
            return;
        }

        bool isEmpty = equipableID == EquipableConfig.EMPTY_SLOT_ID;
        canvasGroup.alpha = isEmpty ? EquipableConfig.ALPHA_EMPTY : EquipableConfig.ALPHA_FILLED;
        m_icon.enabled = !isEmpty;

        if (!isEmpty)
        {
            // Look up the item from the ItemDatabase using the equipable ID
            if (ItemDatabase.TryGetItemFromID(equipableID, out Item item))
            {
                // Use the item's icon if available, otherwise use unknown icon fallback
                bool hasIcon = item.icon != null;
                m_icon.sprite = hasIcon ? item.icon : m_unknownIcon;

                if (!hasIcon)
                {
                    DbsContentApi.Modules.Logger.LogWarning($"[EquipableUI] Item '{item.displayName}' (ID: {equipableID}) has no icon sprite.");
                }
            }
            else
            {
                // Item not found in database, use unknown icon
                m_icon.sprite = m_unknownIcon;
                DbsContentApi.Modules.Logger.LogError($"[EquipableUI] Item with ID {equipableID} not found in ItemDatabase.");
            }
        }
    }
}
