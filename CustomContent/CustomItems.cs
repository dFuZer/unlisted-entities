using UnityEngine;
using DbsContentApi.Modules;
using Modules.Logger.Patches;
using System;
using Photon.Pun;
using Zorro.Core;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Handles the setup and registration of specific custom items for this mod.
/// Uses the generic Items API.
/// </summary>
public static class CustomItems
{
    private static AssetBundle? _bundle;

    /// <summary>
    /// Configures all custom items using the loaded AssetBundle.
    /// </summary>
    /// <param name="bundle">The AssetBundle containing item assets.</param>
    public static void Setup(AssetBundle bundle)
    {
        _bundle = bundle;
        // Queue registration for when the API is ready
        string[] allAssets = bundle.GetAllAssetNames();
        Debug.Log("Assets in bundle: " + string.Join(", ", allAssets));

        void RegisterItems()
        {
            // RegisterItem("Bat.prefab", 0, ShopItemCategory.Misc, "icon_bat", "Fart sound effect");
            // RegisterItem("Item.prefab", 0, ShopItemCategory.Misc, "icon_bat", "Fart sound effect");

            ContentLoader.RegisterPrefabInPhotonPool(CustomMobs.TeapotFinal);
            RegisterItem("TeapotSpawner.prefab", 0, ShopItemCategory.Misc, "icon_bat", "Fart sound effect");
        }

        DbsContentApi.DbsContentApiPlugin.customItemsRegistrationCallbacks.Add(RegisterItems);
    }


    static void RegisterItem(string prefabName, int price, ShopItemCategory category, string iconName, string soundEffectName)
    {
        Debug.Log($"Registering item: {prefabName}");

        GameObject prefab = LoadPrefab(prefabName);
        SetupPrefab(prefab, prefabName);
        RegisterPrefabInPool(prefab);

        Item item = CreateItem(prefabName, prefab, price, category, iconName, soundEffectName);
        if (!CheckDoubloons(item))
            AddItemToDatabase(item);
        SetAllItemsFree();
    }

    static GameObject LoadPrefab(string prefabName)
    {
        if (_bundle == null)
            throw new Exception("AssetBundle is null");

        GameObject prefab = _bundle.LoadAsset<GameObject>(prefabName);
        if (prefab == null)
            throw new Exception($"{prefabName} not found in AssetBundle");

        return prefab;
    }

    static void SetupPrefab(GameObject prefab, string prefabName)
    {
        ApplyMetalMaterial(prefab); // you can comment this line if you want to add your custom material
        EnsureComponents(prefab, prefabName);
    }

    static Material? FindMaterial(string materialName)
    {
        foreach (Renderer r in UnityEngine.Object.FindObjectsOfType<Renderer>())
        {
            if (r.sharedMaterial != null && r.sharedMaterial.name == materialName)
                return r.sharedMaterial;
        }
        return null;
    }

    static void ApplyMetalMaterial(GameObject target)
    {
        if (target == null) return;

        Material? metalMaterial = FindMaterial("M_Metal");
        if (metalMaterial != null)
        {
            foreach (var renderer in target.GetComponentsInChildren<Renderer>())
            {
                renderer.material = metalMaterial;
            }
        }
    }

    static void EnsureComponents(GameObject prefab, string prefabName)
    {
        // Add ItemInstance component if missing
        if (prefab.GetComponent<ItemInstance>() == null)
        {
            prefab.AddComponent<ItemInstance>();
            Debug.Log("Added ItemInstance component");
        }
        EnsureHandGizmo(prefab);
        // Add custom behaviours
        AddCustomBehaviours(prefab, prefabName);
    }

    static void EnsureHandGizmo(GameObject prefab)
    {
        if (prefab.GetComponentInChildren<HandGizmo>(true) != null) return;

        GameObject handGizmoObj = new GameObject("HandGizmo");
        handGizmoObj.transform.SetParent(prefab.transform);
        handGizmoObj.AddComponent<HandGizmo>();

        // Add dummy child for HandGizmo.Start()
        GameObject dummyChild = new GameObject("GizmoVisual");
        dummyChild.transform.SetParent(handGizmoObj.transform);
        dummyChild.transform.localPosition = Vector3.zero;
        dummyChild.transform.localRotation = Quaternion.identity;

        Debug.Log("Added HandGizmo with dummy child");
    }

    static void AddCustomBehaviours(GameObject prefab, string prefabName)
    {
        if (prefabName == "Item.prefab")
            prefab.AddComponent<LaserBehaviour>(); // add your custom item Behaviour
        else if (prefabName == "Bat.prefab")
            prefab.AddComponent<BatBehaviour>();
        else if (prefabName == "TeapotSpawner.prefab")
            prefab.AddComponent<TeapotSpawnerBehaviour>();
    }

    static void RegisterPrefabInPool(GameObject prefab)
    {
        if (PhotonNetwork.PrefabPool is DefaultPool defaultPool)
        {
            if (!defaultPool.ResourceCache.ContainsKey(prefab.name))
            {
                defaultPool.ResourceCache.Add(prefab.name, prefab);
            }
        }
    }

    static Item CreateItem(string prefabName, GameObject prefab, int price, ShopItemCategory category, string iconName, string soundEffectName)
    {
        ItemDatabase db = SingletonAsset<ItemDatabase>.Instance;
        Item item = ScriptableObject.CreateInstance<Item>();

        SetupPhysicsSound(prefab, db, soundEffectName);
        Setupicon(prefab, item, iconName);
        SetupItemBasics(item, prefabName, prefab, price, category);

        return item;
    }

    static void Setupicon(GameObject prefab, Item item, string iconName)
    {
        Sprite icon = _bundle.LoadAsset<Sprite>(iconName);
        if (icon != null)
            item.icon = icon;
    }

    static void SetupPhysicsSound(GameObject prefab, ItemDatabase db, string soundEffectName)
    {
        var ps = prefab.AddComponent<PhysicsSound>();
        AudioClip customImpactSound = _bundle.LoadAsset<AudioClip>(soundEffectName);

        if (customImpactSound == null)
        {
            Debug.LogWarning("Custom impact sound not found, using fallback");
            ps.impactSounds = GetFallbackPhysicsSound(db);
        }
        else
        {
            ps.impactSounds = CreateCustomImpactSound(customImpactSound, db);
            Debug.Log("Custom impact sound loaded successfully!");
        }
    }

    static SFX_Instance[] GetFallbackPhysicsSound(ItemDatabase db)
    {
        var objectsField = GetObjectsField(db);
        var currentItems = GetItemsFromField(objectsField, db);
        return currentItems[0].itemObject.GetComponent<PhysicsSound>().impactSounds;
    }

    static SFX_Instance[] CreateCustomImpactSound(AudioClip customSound, ItemDatabase db)
    {
        var objectsField = GetObjectsField(db);
        var currentItems = GetItemsFromField(objectsField, db);

        SFX_Instance templateSFX = currentItems[0].itemObject.GetComponent<PhysicsSound>()?.impactSounds?[0];
        if (templateSFX == null)
        {
            Debug.LogError("Could not find template SFX_Instance");
            return GetFallbackPhysicsSound(db);
        }

        SFX_Instance sfxInstance = ScriptableObject.Instantiate(templateSFX);
        sfxInstance.clips = new AudioClip[] { customSound };
        return new SFX_Instance[] { sfxInstance };
    }

    static void SetupItemBasics(Item item, string prefabName, GameObject prefab, int price, ShopItemCategory category)
    {
        item.displayName = prefabName.Replace(".prefab", "");
        item.itemObject = prefab;
        item.persistentID = "minifridgemod." + item.displayName.ToLower();
        item.name = "minifridge." + item.displayName.ToLower();

        item.itemType = Item.ItemType.Tool;
        item.Category = category;

        item.mass = 0.5f;
        item.holdPos = new Vector3(0.3f, -0.3f, 0.7f);
        item.holdRotation = Vector3.zero;
        item.useAlternativeHoldingPos = false;
        item.useAlternativeHoldingRot = false;
        item.groundSizeMultiplier = 1f;
        item.groundMassMultiplier = 1f;

        item.purchasable = true;
        item.price = price;
        item.quantity = 1;
        item.spawnable = true;
        item.toolSpawnRarity = RARITY.common;
        item.toolBudgetCost = 1;
        item.budgetCost = 0;
        item.rarity = 1f;

        item.content = null;
        item.Tooltips = new List<ItemKeyTooltip>();
    }

    static FieldInfo GetObjectsField(ItemDatabase db)
    {
        var objectsField = db.GetType().GetField("Objects", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (objectsField == null)
            objectsField = db.GetType().GetField("objects", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (objectsField == null)
            throw new Exception("Could not find Objects field in ItemDatabase");

        return objectsField;
    }

    static List<Item> GetItemsFromField(FieldInfo objectsField, ItemDatabase db)
    {
        var objectsValue = objectsField.GetValue(db);

        if (objectsValue is List<Item> itemList)
            return itemList;
        else if (objectsValue is Item[] itemArray)
            return new List<Item>(itemArray);

        throw new Exception($"Objects field is type {objectsValue?.GetType()?.Name ?? "null"}, expected List<Item> or Item[]");
    }

    static Item CopyIconFromTemplate(List<Item> currentItems)
    {
        foreach (Item existingItem in currentItems)
        {
            if (existingItem.icon != null && existingItem.purchasable)
            {
                Debug.Log($"Found template item with icon: {existingItem.displayName}");
                return existingItem;
            }
        }
        Debug.LogWarning("No template item found with icon!");
        return null;
    }

    static void AddItemToDatabase(Item item)
    {
        ItemDatabase db = SingletonAsset<ItemDatabase>.Instance;
        var objectsField = GetObjectsField(db);
        var currentItems = GetItemsFromField(objectsField, db);

        Item templateItem = CopyIconFromTemplate(currentItems);

        item.id = currentItems.Count > 0 ? (byte)(currentItems.Max(i => i.id) + 1) : (byte)0;
        currentItems.Add(item);

        objectsField.SetValue(db, currentItems);
        Debug.Log($"Item '{item.displayName}' registered with ID: {item.id}");
    }

    static void SetAllItemsFree()
    {
        ItemDatabase db = SingletonAsset<ItemDatabase>.Instance;
        var objectsField = GetObjectsField(db);
        var currentItems = GetItemsFromField(objectsField, db);

        foreach (Item existingItem in currentItems)
        {
            if (existingItem != null)
                existingItem.price = 0;
        }

        objectsField.SetValue(db, currentItems);
    }

    static bool CheckDoubloons(Item item)
    {
        ItemDatabase db = SingletonAsset<ItemDatabase>.Instance;
        var objectsField = GetObjectsField(db);
        var currentItems = GetItemsFromField(objectsField, db);

        if (currentItems.Count == 0)
            return false;
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (currentItems[i].displayName == item.displayName)
                return true;
        }
        return false;  // No duplicates
    }

}