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
	public static GameObject? FroggyBootRightPrefab = null;
	public static GameObject? FroggyBootLeftPrefab = null;
	public static GameObject? CursedNecklace = null;

	/// <summary>
	/// Reference to the Jumping Boots item for gameplay checks.
	/// </summary>
	public static Item? JumpingBootsItem = null;

	public static Item? CursedDoll = null;

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

		var oxygenRegenerationSfx = _bundle!.LoadAsset<SFX_Instance>("OxygenRegenSfx")!;

		void RegisterItems()
		{
			RegisterUnbreakableBat();
			RegisterBreakableBat();
			RegisterSmallO2Tank(oxygenRegenerationSfx);
			RegisterLargeO2Tank(oxygenRegenerationSfx);
			RegisterBandAidBox();
			RegisterMedkit();
			RegisterEnergyBar();
			RegisterInvisibilitySpray();
			RegisterGrenade();
			RegisterSemtexGrenade();
			RegisterElectricGrenade();
			RegisterJumpingBoots();
			RegisterCursedDoll();
		}

		DbsContentApiPlugin.customItemsRegistrationCallbacks.Add(RegisterItems);
	}

	private static void RegisterUnbreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Unbreakable Bat.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterialType.M_DARKGRAY);
		BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
		batBehaviour.batHitSFX = _bundle!.LoadAsset<SFX_Instance>("SFX Bat Hit");
		batBehaviour.isBreakable = false;

		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(_bundle!.LoadAsset<AudioClip>("bat_fall"));
		foreach (SFX_Instance sfx in impactSounds)
		{
			sfx.settings.pitch = 0.8f;
			sfx.settings.volume = 0.7f;
		}

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Solid bat",
			price: 500,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_bat",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterBreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Breakable Bat.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterialType.BROWN);
		BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
		batBehaviour.batHitSFX = _bundle.LoadAsset<SFX_Instance>("SFX Bat Hit");
		batBehaviour.isBreakable = true;

		AudioClip batFallClip = _bundle!.LoadAsset<AudioClip>("bat_fall");
		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(batFallClip);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Fragile bat",
			price: 50,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_bat",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterSmallO2Tank(SFX_Instance oxygenRegenerationSfx)
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.ContainerBounce1, ImpactSoundType.ContainerBounce2);
		foreach (SFX_Instance sfx in impactSounds)
		{
			sfx.settings.pitch = 1.5f;
			sfx.settings.volume = 0.3f;
		}

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SmallO2Tank.prefab");
		GameObject visualRoot = prefab.transform.Find("Visual/o2tank")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterialType.M_DARKGRAY, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cap")!.gameObject, GameMaterialType.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rims")!.gameObject, GameMaterialType.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Text")!.gameObject, GameMaterialType.M_Cap_1, false);
		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 30;
		behaviour.statType = StatTypeEnum.Oxygen;
		behaviour.oxygenRegenerationSfx = oxygenRegenerationSfx;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Small O2 tank",
			price: 30,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_o2_tank",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterLargeO2Tank(SFX_Instance oxygenRegenerationSfx)
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.ContainerBounce1, ImpactSoundType.ContainerBounce2);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "LargeO2Tank.prefab");
		GameObject visualRoot = prefab.transform.Find("Visual/o2tank")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterialType.M_DARKGRAY, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cap")!.gameObject, GameMaterialType.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rims")!.gameObject, GameMaterialType.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Text")!.gameObject, GameMaterialType.M_Cap_1, false);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 80;
		behaviour.statType = StatTypeEnum.Oxygen;
		behaviour.oxygenRegenerationSfx = oxygenRegenerationSfx;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Large O2 tank",
			price: 80,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_o2_tank",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterBandAidBox()
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce3);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BandAidBox.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/bandaid")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Barcode")!.gameObject, GameMaterialType.VERY_DARK_GRAY, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cross")!.gameObject, GameMaterialType.RED, false);
		Renderer boxRenderer = visualRoot!.transform.Find("Box")!.gameObject.GetComponent<Renderer>();
		boxRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.BROWN), GameMaterials.GetMaterial(GameMaterialType.WHITE_IVORY), GameMaterials.GetMaterial(GameMaterialType.GRAY_BLUE) };
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Gauze")!.gameObject, GameMaterialType.WHITE, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("PlasterFront")!.gameObject, GameMaterialType.ORANGE_SAUSAGE, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TextBack")!.gameObject, GameMaterialType.VERY_DARK_GRAY, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TextFront")!.gameObject, GameMaterialType.VERY_DARK_GRAY, false);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 40;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Band-aid box",
			price: 30,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_band_aid_box",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterMedkit()
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce3);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Medkit.prefab");
		GameObject visualRoot = prefab.transform.Find("Visual/medkit")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rim")!.gameObject, GameMaterialType.M_Cap_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Case")!.gameObject, GameMaterialType.RED, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cross")!.gameObject, GameMaterialType.M_Cap_1, false);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 100;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Medkit",
			price: 70,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_medkit",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f),
			holdRot: new Vector3(0, 0, 0)
		);
	}

	private static void RegisterEnergyBar()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "EnergyBar.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/energybar")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterialType.M_DARKGRAY, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Bar")!.gameObject, GameMaterialType.M_Beanie_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Icon")!.gameObject, GameMaterialType.M_Cap_1, false);
		var behaviour = prefab.AddComponent<TemporaryPlayerBoostItemBehaviour>();
		behaviour.playerCrunchSFX = _bundle!.LoadAsset<SFX_Instance>("CrunchSfx")!;


		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.BombBounce2);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Energy bar",
			price: 35,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_energybar",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterInvisibilitySpray()
	{
		var invisibilitySpraySfx = _bundle!.LoadAsset<SFX_Instance>("SpraySfx")!;
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "InvisibilitySpray.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/spraycan")!.gameObject;
		GameMaterials.ApplyMaterial(prefab, GameMaterialType.M_DARKGRAY, true);
		GameObject cap = visualRoot!.transform.Find("Cap")!.gameObject;
		Renderer capRenderer = cap.GetComponent<Renderer>();
		capRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.M_Cap_1), GameMaterials.GetMaterial(GameMaterialType.M_DARKGRAY) };
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Icon")!.gameObject, GameMaterialType.YELLOW, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("CanBridge")!.gameObject, GameMaterialType.M_DARKGRAY, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("CapBridge")!.gameObject, GameMaterialType.M_Cap_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Bulb")!.gameObject, GameMaterialType.M_Cap_1, false);
		var behaviour = prefab.AddComponent<TemporaryInvisibilityItemBehaviour>();
		behaviour.invisibilitySpraySfx = invisibilitySpraySfx;


		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce6);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Invisibility spray",
			price: 65,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "icon_invisibility_spray",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterGrenade()
	{
		// Build shared explosion prefab once for all grenades/semtex that use base GrenadeItemBehaviour
		GrenadeItemBehaviour.SharedExplosionPrefab = GrenadeItemBehaviour.BuildExplosionPrefab();

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Grenade.prefab");
		var visualRoot = prefab.transform.Find("Item/grenade")!.gameObject;
		var grenadeTransform = visualRoot.transform.Find("Grenade")!.gameObject;
		Renderer grenadeRenderer = grenadeTransform.GetComponent<Renderer>();
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.DARKGRAY2), GameMaterials.GetMaterial(GameMaterialType.GREEN) };

		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("SmallTriggerRing")!.gameObject, GameMaterialType.DARKGRAY2, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TriggerRing")!.gameObject, GameMaterialType.DARKGRAY2, false);
		var behaviour = prefab.AddComponent<GrenadeItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Grenade",
			price: 65,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_energybar",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterSemtexGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SemtexGrenade.prefab");
		var visualRoot = prefab.transform.Find("Item/semtex")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Nails")!.gameObject, GameMaterialType.DARKGRAY2, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("SmallTriggerRing")!.gameObject, GameMaterialType.DARKGRAY2, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TriggerRing")!.gameObject, GameMaterialType.DARKGRAY2, false);

		var grenadeRenderer = visualRoot.transform.Find("Grenade")!.gameObject.GetComponent<Renderer>();
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.GREEN), GameMaterials.GetMaterial(GameMaterialType.YELLOW), GameMaterials.GetMaterial(GameMaterialType.DARKGRAY2), GameMaterials.GetMaterial(GameMaterialType.WHITE) };

		var behaviour = prefab.AddComponent<SemtexItemBehaviour>();
		behaviour.hasTickingSound = true;
		behaviour.onStickSfx = _bundle!.LoadAsset<SFX_Instance>("SemtexStickSfx");
		behaviour.tickingSoundClip = _bundle!.LoadAsset<AudioClip>("semtex-tick");
		behaviour.baseTickingPitch = 0.8f;
		behaviour.tickingPitchMultiplier = 0.25f;
		behaviour.maxTickingPitch = 8f;
		behaviour.minTickingPitch = 1f;
		behaviour.tickingVolume = 0.35f;

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Semtex grenade",
			price: 65,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_energybar",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterElectricGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricGrenade.prefab");
		var visualRoot = prefab.transform.Find("Item/electric-grenade")!.gameObject;
		var grenadeRenderer = visualRoot.transform.Find("Grenade")!.gameObject.GetComponent<Renderer>();
		// gray, luminous white, dark gray
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.GRAY), GameMaterials.GetMaterial(GameMaterialType.BRIGHT_WHITE), GameMaterials.GetMaterial(GameMaterialType.DARKGRAY2) };

		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("InnerRods")!.gameObject, GameMaterialType.BRIGHT_WHITE, false);

		var partLRenderer = visualRoot.transform.Find("Part_L")!.gameObject.GetComponent<Renderer>();
		var partRRenderer = visualRoot.transform.Find("Part_R")!.gameObject.GetComponent<Renderer>();
		// for partL : darkgray, light, gray, flatgray
		partLRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.DARKGRAY2), GameMaterials.GetMaterial(GameMaterialType.BRIGHT_WHITE), GameMaterials.GetMaterial(GameMaterialType.GRAY), GameMaterials.GetMaterial(GameMaterialType.FLAT_GRAY) };
		// for partR : darkgray, gray, flatgray, light
		partRRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.DARKGRAY2), GameMaterials.GetMaterial(GameMaterialType.GRAY), GameMaterials.GetMaterial(GameMaterialType.FLAT_GRAY), GameMaterials.GetMaterial(GameMaterialType.BRIGHT_WHITE) };

		var explosionPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricExplosion.prefab");
		ElectricGrenadeExplosionAOE explosionAOE = explosionPrefab.AddComponent<ElectricGrenadeExplosionAOE>();

		var behaviour = prefab.AddComponent<ElectricGrenadeItemBehaviour>();
		behaviour.electricExplosionPrefab = explosionPrefab;
		behaviour.baseTickingPitch = 0.8f;
		behaviour.tickingPitchMultiplier = 0.25f;
		behaviour.maxTickingPitch = 8f;
		behaviour.minTickingPitch = 1f;
		behaviour.hasTickingSound = true;
		behaviour.electricExplosionSfx = _bundle!.LoadAsset<SFX_Instance>("ElectricGrenadeExplosionSfx");
		behaviour.tickingSoundClip = _bundle!.LoadAsset<AudioClip>("electric-grenade-tick");
		behaviour.tickingVolume = 0.35f;

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Electric grenade",
			price: 65,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "icon_energybar",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}


	private static void RegisterJumpingBoots()
	{
		FroggyBootRightPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootRight.prefab");
		GameMaterials.ApplyMaterial(FroggyBootRightPrefab, GameMaterialType.GREEN2, true);
		FroggyBootLeftPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootLeft.prefab");
		GameMaterials.ApplyMaterial(FroggyBootLeftPrefab, GameMaterialType.GREEN2, true);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BootsEquipable.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterialType.GREEN2);
		prefab.AddComponent<BootsEquipableItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		JumpingBootsItem = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Froggy boots",
			price: 100,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "icon_froggy_boots",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	private static void RegisterCursedDoll()
	{
		CursedNecklace = ContentLoader.LoadPrefabFromBundle(_bundle!, "CursedNecklace.prefab");
		GameMaterials.ApplyMaterial(CursedNecklace, GameMaterialType.GREEN2, true);

		var visualRoot = CursedNecklace.transform.Find("CursedNecklace")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Torus")!.gameObject, GameMaterialType.FLAT_GRAY, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Mesh_0")!.gameObject, GameMaterialType.BEIGE2, false);

		var dollRenderer = visualRoot.transform.Find("Mesh_0")!.gameObject.GetComponent<Renderer>();
		dollRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.BEIGE2) };
		var necklaceRenderer = visualRoot.transform.Find("Torus")!.gameObject.GetComponent<Renderer>();
		necklaceRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterialType.FLAT_GRAY) };

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "DollItem.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterialType.BEIGE2, true);
		prefab.AddComponent<BootsEquipableItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		CursedDoll = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Cursed Doll",
			price: 100,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "icon_froggy_boots",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	public static Sprite GetSprite(string iconName)
	{
		return _bundle!.LoadAsset<Sprite>(iconName);
	}
}