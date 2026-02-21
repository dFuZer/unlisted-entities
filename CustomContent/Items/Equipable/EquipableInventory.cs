using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

/// <summary>
/// Manages a player's equipable items inventory. Each player has a fixed number of equipable slots
/// that persist across the network via Photon RPC calls. Equipable items are identified by their item ID.
/// </summary>
public class EquipableInventory : MonoBehaviourPun
{
    // Simple slot system: 0 = Boots, 1 = Charm (or generic)
    public byte[] equipableIDs = new byte[EquipableConfig.SLOT_COUNT];

    private Dictionary<int, GameObject> spawnedVisuals = new Dictionary<int, GameObject>();
    private Player? cachedPlayer;

    private void Awake()
    {
        // Initialize all slots to empty
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            equipableIDs[i] = EquipableConfig.EMPTY_SLOT_ID;
        }
    }

    private Player? GetPlayer()
    {
        if (cachedPlayer != null) return cachedPlayer;

        // EquipableInventory is on GlobalPlayerData which is on a GameObject representing the player
        // We need to find the actual Player component.
        // In this game, GlobalPlayerData is often associated with the PhotonPlayer.
        // Let's look for a Player component in the scene that belongs to the same PhotonView owner.
        if (PlayerHandler.instance == null || PlayerHandler.instance.players == null) return null;

        foreach (var p in PlayerHandler.instance.players)
        {
            if (p != null && p.refs.view != null && p.refs.view.Owner == photonView.Owner)
            {
                cachedPlayer = p;
                return p;
            }
        }
        return null;
    }

    private void LateUpdate()
    {
        // Check if we have equipped items but no spawned visuals
        bool needsUpdate = false;
        for (int i = 0; i < equipableIDs.Length; i++)
        {
            if (equipableIDs[i] != EquipableConfig.EMPTY_SLOT_ID)
            {
                if (!spawnedVisuals.TryGetValue(i, out GameObject visual) || visual == null)
                {
                    needsUpdate = true;
                    break;
                }
            }
            else
            {
                // If slot is empty but we have a visual, we also need an update (cleanup)
                if (spawnedVisuals.TryGetValue(i, out GameObject visual) && visual != null)
                {
                    needsUpdate = true;
                    break;
                }
            }
        }

        if (needsUpdate)
        {
            // Reset cached player in case it was destroyed/re-instantiated
            cachedPlayer = null;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        Player? player = GetPlayer();
        if (player == null)
        {
            // Cleanup visuals if player is gone
            foreach (var kvp in spawnedVisuals)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            spawnedVisuals.Clear();
            return;
        }



        for (int i = 0; i < equipableIDs.Length; i++)
        {
            byte itemID = equipableIDs[i];

            // Handle cleanup of existing visual if it doesn't match or is null
            if (spawnedVisuals.TryGetValue(i, out GameObject existing))
            {
                if (existing == null || itemID == EquipableConfig.EMPTY_SLOT_ID)
                {
                    if (existing != null) Destroy(existing);
                    spawnedVisuals.Remove(i);
                }
                // If ID matches, we could check if it's still parented correctly, 
                // but for now let's assume if it exists and is not null, it's fine.
                // The watchdog handles the null case.
            }

            if (itemID == EquipableConfig.EMPTY_SLOT_ID) continue;

            // Only spawn if we don't already have a valid visual for this slot
            if (!spawnedVisuals.ContainsKey(i))
            {
                // Spawn logic for specific items
                if (UnlistedEntities.CustomContent.CustomItems.JumpingBootsItem != null &&
                    itemID == UnlistedEntities.CustomContent.CustomItems.JumpingBootsItem.id)
                {
                    SpawnFroggyBoot(i, player);
                }
                else if (UnlistedEntities.CustomContent.CustomItems.CursedDoll != null &&
                    itemID == UnlistedEntities.CustomContent.CustomItems.CursedDoll.id)
                {
                    SpawnCursedDoll(i, player);
                }
                else if (UnlistedEntities.CustomContent.CustomItems.AngelWingsItem != null &&
                    itemID == UnlistedEntities.CustomContent.CustomItems.AngelWingsItem.id)
                {
                    SpawnAngelWings(i, player);
                }
                else if (UnlistedEntities.CustomContent.CustomItems.StrongArmItem != null &&
                    itemID == UnlistedEntities.CustomContent.CustomItems.StrongArmItem.id)
                {
                    SpawnStrongArm(i, player);
                }
            }
        }
    }

    private GameObject SpawnAngelWings(int slot, Player player)
    {
        if (UnlistedEntities.CustomContent.CustomItems.AngelWingsPrefab == null) return null;
        Transform? torso = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Torso");

        if (torso != null)
        {
            GameObject wings = Instantiate(UnlistedEntities.CustomContent.CustomItems.AngelWingsPrefab, torso);
            wings.transform.localPosition = new UnityEngine.Vector3(0f, 0.65f, -1.64f);

            wings.transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, 0f);
            wings.transform.localScale = new UnityEngine.Vector3(2.73f, 2.73f, 2.73f);
            spawnedVisuals[slot] = wings;
            var playerShader = player.gameObject.transform.Find("CharacterModel/BodyRenderer").GetComponent<Renderer>().material.shader;
            foreach (var renderer in wings.GetComponentsInChildren<Renderer>())
            {
                renderer.material.shader = playerShader;
            }
            return wings;
        }
        return null;
    }

    private void SpawnCursedDoll(int slot, Player player)
    {
        if (UnlistedEntities.CustomContent.CustomItems.CursedNecklacePrefab == null) return;
        Transform? torso = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Torso");

        if (torso != null)
        {
            GameObject necklace = Instantiate(UnlistedEntities.CustomContent.CustomItems.CursedNecklacePrefab, torso);
            necklace.transform.localPosition = new UnityEngine.Vector3(1.94f, 1.80f, 2.47f);

            necklace.transform.localRotation = UnityEngine.Quaternion.Euler(-83.044f, 0, 0);
            necklace.transform.localScale = new UnityEngine.Vector3(2.98f, 2.98f, 2.98f);
            spawnedVisuals[slot] = necklace;
            var playerShader = player.gameObject.transform.Find("CharacterModel/BodyRenderer").GetComponent<Renderer>().material.shader;
            foreach (var renderer in necklace.GetComponentsInChildren<Renderer>())
            {
                renderer.material.shader = playerShader;
            }
        }
        else
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find Torso bone for necklace attachment.");
        }
    }

    /// When remapping the bones of a prefab's skinned mesh renderer, it is critical that the bones of our prefab map to the bones of the player
    /// and in the order that is expected by the prefab.
    /// After extracting the player fbx from AssetStudio then using its armature to create our Strong arm visual prefab
    /// we logged the prefab's bone order and the player's bone order.
    /// Since there were many mismatches, we create a new array manually mapping the player transforms to the prefab's bone order.
    /// This function should work as long as our exported fbx armature keep the same bone order.
    /// Mind that our bone order MAY be different from one fbx to another. If you try to
    /// extract the player fbx from AssetStudio then use its armature to create our Strong arm visual prefab,
    /// you may have to update this function to match YOUR prefabs' armatures bone order.
    public static Transform[] getPlayerRigTransformsAsExpectedByDbExportedFbx(Player player)
    {
        var playerBones = new List<Transform>
        {
            player.transform.Find("RigCreator/Rig/Armature/Hip"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Extra_1"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_L/Knee_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_L/Knee_L/Foot_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_R/Knee_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Leg_R/Knee_R/Foot_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Index_1_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Index_1_L/Finger_Index_2_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Index_1_L/Finger_Index_2_L/Finger_Index_3_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Middle_1_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Middle_1_L/Finger_Middle_2_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Middle_1_L/Finger_Middle_2_L/Finger_Middle_3_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Pinky_1_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Pinky_1_L/Finger_Pinky_2_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Pinky_1_L/Finger_Pinky_2_L/Finger_Pinky_3_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Ring_1_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Ring_1_L/Finger_Ring_2_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Finger_Ring_1_L/Finger_Ring_2_L/Finger_Ring_3_L"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Thumb_L_1"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Thumb_L_1/Thumb_L_2"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L/Thumb_L_1/Thumb_L_2/Thumb_L_3"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Index_1_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Index_1_R/Finger_Index_2_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Index_1_R/Finger_Index_2_R/Finger_Index_3_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Middle_1_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Middle_1_R/Finger_Middle_2_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Middle_1_R/Finger_Middle_2_R/Finger_Middle_3_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Pinky_1_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Pinky_1_R/Finger_Pinky_2_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Pinky_1_R/Finger_Pinky_2_R/Finger_Pinky_3_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Ring_1_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Ring_1_R/Finger_Ring_2_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Finger_Ring_1_R/Finger_Ring_2_R/Finger_Ring_3_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Thumb_1_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Thumb_1_R/Thumb_2_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R/Thumb_1_R/Thumb_2_R/Thumb_3_R"),
            player.transform.Find("RigCreator/Rig/Armature/Hip/Torso/Head"),
        };

        return playerBones.ToArray();
    }

    private void SpawnStrongArm(int slot, Player player)
    {
        var bodyRenderer = player.gameObject.transform.Find("CharacterModel/BodyRenderer")?.GetComponent<SkinnedMeshRenderer>();
        var prefab = UnlistedEntities.CustomContent.CustomItems.StrongArmVisualPrefab;

        if (bodyRenderer == null || prefab == null) return;

        var playerBones = getPlayerRigTransformsAsExpectedByDbExportedFbx(player);

        // 4. Instantiate and apply
        Transform characterModel = player.gameObject.transform.Find("CharacterModel");
        GameObject strongArm = Instantiate(prefab, characterModel);

        // Reset transforms - usually 0,0,0 if bones are synced, but keeping your values
        strongArm.transform.localPosition = new UnityEngine.Vector3(3.46f, 8.905f, 0.12f);
        strongArm.transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, 0f);
        strongArm.transform.localScale = new UnityEngine.Vector3(7.37f, 7.37f, 7.37f);

        var instanceSMR = strongArm.GetComponentInChildren<SkinnedMeshRenderer>(true);
        instanceSMR.bones = playerBones; // Use the reordered array
        instanceSMR.rootBone = bodyRenderer.rootBone;

        // Shader sync
        foreach (var renderer in strongArm.GetComponentsInChildren<Renderer>())
        {
            renderer.material.shader = bodyRenderer.material.shader;
        }

        spawnedVisuals[slot] = strongArm;
    }

    private void SpawnFroggyBoot(int slot, Player player)
    {
        if (UnlistedEntities.CustomContent.CustomItems.FroggyBootRightPrefab == null) return;
        if (UnlistedEntities.CustomContent.CustomItems.FroggyBootLeftPrefab == null) return;

        // Path: Player/RigCreator/Rig/Armature/Hip/Leg_R/Knee_R/Foot_R/
        // player.refs.rigRoot is "RigCreator"
        Transform? footR = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Leg_R/Knee_R/Foot_R");
        Transform? footL = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Leg_L/Knee_L/Foot_L");


        if (footR != null)
        {
            GameObject boot = Instantiate(UnlistedEntities.CustomContent.CustomItems.FroggyBootRightPrefab, footR);
            boot.transform.localPosition = new UnityEngine.Vector3(0.5087553f, 0.7142437f, 0.01682295f);
            // 270 270 0
            boot.transform.localRotation = UnityEngine.Quaternion.Euler(-90, 0, -90);
            boot.transform.localScale = new UnityEngine.Vector3(2.398091f, 2.398091f, 2.398091f);
            spawnedVisuals[slot] = boot;

            boot.GetComponent<Renderer>().material.shader = player.gameObject.transform.Find("CharacterModel/BodyRenderer").GetComponent<Renderer>().material.shader;
            // HelperFunctions.SetChildRendererLayer(boot.transform, 29);
        }
        else
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find Foot_R bone for Froggy Boot attachment.");
        }

        if (footL != null)
        {
            GameObject boot = Instantiate(UnlistedEntities.CustomContent.CustomItems.FroggyBootLeftPrefab, footL);
            boot.transform.localPosition = new UnityEngine.Vector3(0.5087553f, 0.7142437f, -0.07f);
            // 270 270 0
            boot.transform.localRotation = UnityEngine.Quaternion.Euler(-90, 0, -90);
            boot.transform.localScale = new UnityEngine.Vector3(2.398091f, 2.398091f, 2.398091f);
            spawnedVisuals[slot] = boot;

            boot.GetComponent<Renderer>().material.shader = player.gameObject.transform.Find("CharacterModel/BodyRenderer").GetComponent<Renderer>().material.shader;
            // HelperFunctions.SetChildRendererLayer(boot.transform, 29);
        }
        else
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find Foot_R bone for Froggy Boot attachment.");
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
            var player = GetPlayer();
            if (player != null)
            {
                var cache = PlayerCache.GetCache(player);
                cache.UpdateData();
            }
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

    private void OnDestroy()
    {
        foreach (var visual in spawnedVisuals.Values)
        {
            if (visual != null) Destroy(visual);
        }
        spawnedVisuals.Clear();
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

    public static bool PlayerHasEquipable(Player player, byte itemID)
    {
        if (!player.TryGetInventory(out var inventory))
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find Inventory component on player.");
            return false;
        }

        var equipables = inventory.gameObject.GetComponent<EquipableInventory>();
        if (equipables == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find EquipableInventory component on player.");
            return false;
        }

        return equipables.HasEquipable(itemID);
    }

    public static bool PlayerHasEquipableCached(Player player, byte itemID)
    {
        var extendedData = PlayerCache.GetCache(player);
        if (extendedData == null)
        {
            DbsContentApi.Modules.Logger.LogError("[EquipableInventory] Could not find ExtendedData component on player.");
            return false;
        }

        return extendedData.PlayerHasEquipable(itemID);
    }
}

/// <summary>
/// Harmony patches for integrating the EquipableInventory system with the game's player lifecycle.
/// </summary>
[HarmonyPatch]
public class EquipableInventoryPatches
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