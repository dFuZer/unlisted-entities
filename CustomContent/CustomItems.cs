using UnityEngine;
using DbsContentApi.Modules;
using System;
using DbsContentApi;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Handles the setup and registration of specific custom items for this mod.
/// Uses the generic Items API.
/// </summary>
public static class CustomItems
{
	private static AssetBundle? _bundle;
	public static byte? WeaponsCategory = null;
	public static byte? ConsumablesCategory = null;
	public static byte? EquipablesCategory = null;

	/// <summary>
	/// Reference to the Jumping Boots item for gameplay checks.
	/// </summary>
	public static Item? JumpingBootsItem = null;

	/// <summary>
	/// Configures all custom items using the loaded AssetBundle.
	/// </summary>
	/// <param name="bundle">The AssetBundle containing item assets.</param>
	public static void Setup(AssetBundle bundle)
	{
		_bundle = bundle;

		// Queue registration for when the API is ready
		string[] allAssets = bundle.GetAllAssetNames();

		WeaponsCategory = Items.RegisterCustomCategory("Weapons");
		ConsumablesCategory = Items.RegisterCustomCategory("Consumables");
		EquipablesCategory = Items.RegisterCustomCategory("Equipables");

		void RegisterItems()
		{
			RegisterUnbreakableBat();
			RegisterBreakableBat();
			RegisterRegenConsumables();
			RegisterAdrenoShot();
			RegisterInvisibilityPotion();
			RegisterJumpingBoots();
		}

		DbsContentApi.DbsContentApiPlugin.customItemsRegistrationCallbacks.Add(RegisterItems);
	}

	/// <summary>
	/// Registers the Bat item.
	/// </summary>
	private static void RegisterBreakableBat()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "Breakable Bat.prefab",
			displayName: "Fragile Bat",
			price: 50,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Cowboy,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
				batBehaviour.batHitSFX = _bundle.LoadAsset<SFX_Instance>("SFX Bat Hit");
				batBehaviour.isBreakable = true;
			}
		);
	}

	private static void RegisterJumpingBoots()
	{
		JumpingBootsItem = Items.RegisterItem(
			bundle: _bundle,
			prefabName: "BootsEquipable.prefab",
			displayName: "Jumping Boots",
			price: 50,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Cowboy,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				BootsEquipableItemBehaviour itemBehaviour = prefab.AddComponent<BootsEquipableItemBehaviour>();
			}
		);
	}

	private static void RegisterInvisibilityPotion()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "InvisibilityPotion.prefab",
			displayName: "Invisibility Potion",
			price: 60,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				TemporaryInvisibilityItemBehaviour itemBehaviour = prefab.AddComponent<TemporaryInvisibilityItemBehaviour>();
			}
		);
	}

	public static Sprite GetSprite(string iconName)
	{
		return _bundle.LoadAsset<Sprite>(iconName);
	}


	private static void RegisterUnbreakableBat()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "Unbreakable Bat.prefab",
			displayName: "Solid Bat",
			price: 500,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
				batBehaviour.batHitSFX = _bundle.LoadAsset<SFX_Instance>("SFX Bat Hit");
				batBehaviour.isBreakable = false;
			}
		);
	}

	private static void RegisterAdrenoShot()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "AdrenoShot.prefab",
			displayName: "Adreno-Shot",
			price: 60,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				TemporaryPlayerBoostItemBehaviour itemBehaviour = prefab.AddComponent<TemporaryPlayerBoostItemBehaviour>();
			}
		);
	}

	private static void RegisterRegenConsumables()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "O2Canister.prefab",
			displayName: "O2 Canister",
			price: 30,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				RegenStatItemBehaviour itemBehaviour = prefab.AddComponent<RegenStatItemBehaviour>();
				itemBehaviour.regenPercentageAmount = 30;
				itemBehaviour.statType = StatTypeEnum.Oxygen;
			}
		);
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "IndustrialO2Tank.prefab",
			displayName: "Industrial O2 Tank",
			price: 70,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				RegenStatItemBehaviour itemBehaviour = prefab.AddComponent<RegenStatItemBehaviour>();
				itemBehaviour.regenPercentageAmount = 80;
				itemBehaviour.statType = StatTypeEnum.Oxygen;
			}
		);
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "MedShot.prefab",
			displayName: "Med Shot",
			price: 30,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				RegenStatItemBehaviour itemBehaviour = prefab.AddComponent<RegenStatItemBehaviour>();
				itemBehaviour.regenPercentageAmount = 40;
				itemBehaviour.statType = StatTypeEnum.Health;
			}
		);
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "TraumaKit.prefab",
			displayName: "Trauma Kit",
			price: 70,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_bat",
			soundEffectName: "bat_fall",
			mat: GameMaterialType.M_Milk1,
			holdPos: new Vector3(0.3f, 0.1f, 0.4f),
			customBehaviourSetup: (prefab, prefabName) =>
			{
				RegenStatItemBehaviour itemBehaviour = prefab.AddComponent<RegenStatItemBehaviour>();
				itemBehaviour.regenPercentageAmount = 100;
				itemBehaviour.statType = StatTypeEnum.Health;
			}
		);
	}
}