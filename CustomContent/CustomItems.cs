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
	public static byte? WeaponsCategory = null;

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


		void RegisterItems()
		{
			RegisterUnbreakableBat();
			RegisterBreakableBat();
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

	private static void RegisterUnbreakableBat()
	{
		Items.RegisterItem(
			bundle: _bundle,
			prefabName: "Unbreakable Bat.prefab",
			displayName: "Metal Bat",
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
}