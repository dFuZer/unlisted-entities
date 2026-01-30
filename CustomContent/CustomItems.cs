using UnityEngine;
using DbsContentApi.Modules;
using System;

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

        void RegisterItems()
        {
            RegisterBat();
            RegisterGenericItem();
            // Uncomment when ready:
            // RegisterTeapotSpawner();
        }

        DbsContentApi.DbsContentApiPlugin.customItemsRegistrationCallbacks.Add(RegisterItems);
    }

    /// <summary>
    /// Registers the Bat item.
    /// </summary>
    private static void RegisterBat()
    {
        Items.RegisterItem(
            bundle: _bundle,
            prefabName: "Bat.prefab",
            price: 0,
            category: ShopItemCategory.Misc,
            iconName: "icon_bat",
            soundEffectName: "bat-falling",
            applyMetalMaterial: true,
            customBehaviourSetup: (prefab, prefabName) =>
            {
                prefab.AddComponent<BatBehaviour>();
            } // optional parameter
        );
        
        Items.SetAllItemsFree();
    }

    /// <summary>
    /// Registers the generic Item.
    /// </summary>
    private static void RegisterGenericItem()
    {
        Items.RegisterItem(
            bundle: _bundle,
            prefabName: "Item.prefab",
            price: 0,
            category: ShopItemCategory.Misc,
            iconName: "icon_bat",
            soundEffectName: "Fart sound effect",
            applyMetalMaterial: true,
            customBehaviourSetup: (prefab, prefabName) =>
            {
                prefab.AddComponent<LaserBehaviour>();
            }
        );
        
        Items.SetAllItemsFree();
    }

    /// <summary>
    /// Registers the TeapotSpawner item.
    /// Uncomment when TeapotFinal is properly registered in photon pool.
    /// </summary>
    private static void RegisterTeapotSpawner()
    {
        // First ensure TeapotFinal is registered in photon pool
        // ContentLoader.RegisterPrefabInPhotonPool(CustomMobs.TeapotFinal);
        
        Items.RegisterItem(
            bundle: _bundle,
            prefabName: "TeapotSpawner.prefab",
            price: 0,
            category: ShopItemCategory.Misc,
            iconName: "icon_bat",
            soundEffectName: "Fart sound effect",
            applyMetalMaterial: true,
            customBehaviourSetup: (prefab, prefabName) =>
            {
                prefab.AddComponent<TeapotSpawnerBehaviour>();
            }
        );
        
        Items.SetAllItemsFree();
    }
}