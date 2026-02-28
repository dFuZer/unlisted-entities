using UnityEngine;
using DbsContentApi.Modules;
using System;
using DbsContentApi;
using System.Linq;

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
	public static byte? ExplosivesCategory = null;
	public static byte? MaterialTestersCategory = null;
	public static GameObject? FroggyBootRightPrefab = null;
	public static GameObject? FroggyBootLeftPrefab = null;
	public static GameObject? AngelWingsPrefab = null;
	public static GameObject? CursedNecklacePrefab = null;
	public static GameObject? GlowingVestPrefab = null;
	public static GameObject? SmallLightBeamPrefab = null;
	public static Item? JumpingBootsItem = null;
	public static Item? CursedDoll = null;
	public static Item? AngelWingsItem = null;
	public static Item? GlowingVest = null;

	private static GameObject GetMonsterAffectingExplosionTemplate(float fall = 3f, float radius = 4f, float damage = 150f, float force = 4f)
	{
		Item? bombItem = Items.GetItemByPrefabComponent<BombItem>();
		if (bombItem == null || bombItem.itemObject == null)
		{
			DbsContentApi.Modules.Logger.Log("CustomItems: Could not find BombItem to copy explosion from!");
			return null!;
		}

		BombItem? bombItemBehaviour = bombItem.itemObject.GetComponent<BombItem>();
		if (bombItemBehaviour == null || bombItemBehaviour.explosion == null)
		{
			DbsContentApi.Modules.Logger.Log("CustomItems: BombItem has no explosion prefab!");
			return null!;
		}

		// Create a new instance for each call so we don't modify a shaM_Jester_4 template
		GameObject explosionInstance = UnityEngine.Object.Instantiate(bombItemBehaviour.explosion);
		explosionInstance.SetActive(false);
		UnityEngine.Object.DontDestroyOnLoad(explosionInstance);

		// Remove original AOE and add custom one
		var aoe = explosionInstance.GetComponent<AOE>();
		if (aoe != null) UnityEngine.Object.DestroyImmediate(aoe);
		var newAoe = explosionInstance.AddComponent<MonsterAffectingAOE>();
		newAoe.fall = fall;
		newAoe.radius = radius;
		newAoe.damage = damage;
		newAoe.force = force;

		return explosionInstance;
	}

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
		ExplosivesCategory = Items.RegisterCustomCategory("Explosives");
		MaterialTestersCategory = Items.RegisterCustomCategory("Material Testers");

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
			RegisterAngelWings();
			RegisterPopit();
			RegisterSilverFulminate();
			RegisterGlowingVest();
			RegisterTranqGun();
			// RegisterMaterialTesters();
		}

		DbsContentApiPlugin.customItemsRegistrationCallbacks.Add(RegisterItems);
	}

	private static void RegisterMaterialTesters()
	{
		for (int i = 0; i < 8; i++)
		{
			// we load MaterialTester.prefab, duplicate it, and apply the material PAGE*8 + i to the duplicate
			// this allows us to test all materials in the game one by one using shop items

			var loadedMaterials = GameMaterials._materials.Keys.ToList();
			DbsContentApi.Modules.Logger.Log($"Loaded {loadedMaterials.Count} materials: {string.Join(", ", loadedMaterials.Select(m => m.ToString()))}");
			GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "MaterialTester.prefab");
			GameObject duplicate = UnityEngine.Object.Instantiate(prefab);
			var ithMaterial = GameMaterials._materials.Keys.ElementAt(i);
			GameMaterials.ApplyMaterial(duplicate, ithMaterial);
			var materialName = ithMaterial.ToString();
			Items.RegisterItem(
				bundle: _bundle!,
				prefab: duplicate,
				displayName: $"{materialName}",
				price: 0,
				category: (ShopItemCategory)MaterialTestersCategory!,
				iconName: $"popit_icon",
				impactSounds: new SFX_Instance[] { },
				holdPos: new Vector3(0.3f, -0.3f, 0.7f),
				holdRot: new Vector3(0, 0, 0)
			);
		}
	}


	private static void RegisterTranqGun()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "TranqGun2.prefab");
		var visualRoot = prefab.transform.Find("TranqGun")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Barrel")!.gameObject, GameMaterial.M_Metal);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Lower")!.gameObject, GameMaterial.M_Player);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Magazine")!.gameObject, GameMaterial.M_Balaclava);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Rear_Sights")!.gameObject, GameMaterial.M_Metal);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Red_Dot_Sight")!.gameObject, GameMaterial.M_Metal);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Slide")!.gameObject, GameMaterial.M_Balaclava);
		GameMaterials.ApplyMaterial(visualRoot.transform.Find("Trigger")!.gameObject, GameMaterial.M_Metal);

		// Configure TranqGun behaviour
		var behaviour = prefab.AddComponent<TranqGunItemBehaviour>();
		behaviour.firePoint = prefab.transform.Find("FirePoint");

		SFX_Instance shotSfx = _bundle!.LoadAsset<SFX_Instance>("TranqGunShotSfx");

		// Load and configure projectile prefab
		GameObject shotPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "TranqShot.prefab");
		var shotPrefabVisualRoot = shotPrefab.transform.Find("wrapper")!.gameObject;
		GameMaterials.ApplyMaterial(shotPrefabVisualRoot.transform.Find("glass")!.gameObject, GameMaterial.M_ShopGlass);
		GameMaterials.ApplyMaterial(shotPrefabVisualRoot.transform.Find("liquid")!.gameObject, GameMaterial.M_Child_2);
		GameMaterials.ApplyMaterial(shotPrefabVisualRoot.transform.Find("shell")!.gameObject, GameMaterial.M_Metal);

		SFX_PlayOneShot shotSfxComponent = shotPrefab.AddComponent<SFX_PlayOneShot>();
		shotSfxComponent.playOnStart = true;
		shotSfxComponent.playOnEnable = false;
		shotSfxComponent.playOnClick = false;
		shotSfxComponent.sfx = shotSfx;
		shotSfxComponent.sfxs = new SFX_Instance[] { shotSfx };

		// Add components at runtime to the shotPrefab
		var proj = shotPrefab.AddComponent<Projectile>();
		proj.velocity = 20f;
		proj.gravity = 8f;
		proj.upVelocity = 1f;
		proj.fall = 1f;
		proj.damage = 0f; // Tranq gun doesn't deal direct damage
		proj.force = 1f;
		proj.postHitBehavior = Projectile.PostHitBehaviour.Disable;
		proj.layerType = HelperFunctions.LayerType.All;

		shotPrefab.AddComponent<TranqProjectileLogic>();

		var stickComponent = shotPrefab.AddComponent<ProjectileStick>();
		var syringeHitSfx = _bundle!.LoadAsset<SFX_Instance>("SyringeHitSfx");
		if (syringeHitSfx == null)
		{
			DbsContentApi.Modules.Logger.LogError("CustomItems: SyringeHitSfx not found!");
		}
		stickComponent.hitSFX = new SFX_Instance[] { syringeHitSfx };

		var remove = shotPrefab.AddComponent<RemoveAfterSeconds>();
		remove.seconds = 10f;

		behaviour.projectilePrefab = shotPrefab;

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Tranquilizer gun",
			price: 200,
			category: (ShopItemCategory)WeaponsCategory!,
			iconName: "tranqgun_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f),
			holdRot: new Vector3(0, 0, 0),
			useAlternativeHoldPos: true,
			useAlternativeHoldRot: false,
			alternativeHoldingPos: new Vector3(0.2f, -0.22f, 0.7f)
		);
	}
	private static void RegisterPopit()
	{
		// Build shaM_Jester_4 explosion prefab once for all grenades/semtex that use base GrenadeItemBehaviour

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Popit.prefab");
		var visualRoot = prefab.transform.Find("Item/PopIt")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterial.M_Child_1, true);

		var behaviour = prefab.AddComponent<GrenadeItemBehaviour>();
		behaviour.explodesOnImpact = true;

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 1.5f, radius: 1f, damage: 10f, force: 2.5f);
		AddGamefeel popitGamefeel = behaviour.explosionPrefab.GetComponent<AddGamefeel>();
		if (popitGamefeel != null)
		{
			popitGamefeel.perlinAmount = 5f;
			popitGamefeel.perlinDuration = 0.5f;
			popitGamefeel.scale = 5f;
			popitGamefeel.range = 50f;
		}
		ParticleSystem explosionParticle = behaviour.explosionPrefab.GetComponentInChildren<ParticleSystem>(true);
		ParticleSystem.EmissionModule emission = explosionParticle.emission;
		emission.SetBurst(0, new ParticleSystem.Burst(0f, 14));
		var particlesMain = explosionParticle.main;
		particlesMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3f);
		particlesMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
		particlesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);

		SFX_PlayOneShot spos = behaviour.explosionPrefab.GetComponent<SFX_PlayOneShot>();
		spos.sfx = _bundle!.LoadAsset<SFX_Instance>("PopitExplosionSfx");
		spos.sfxs = new SFX_Instance[] { };

		Transform explosionLight = behaviour.explosionPrefab.transform.Find("Point Light");
		if (explosionLight != null)
		{
			// Use DestroyImmediate in Editor scripts, otherwise use Destroy
			UnityEngine.Object.Destroy(explosionLight.gameObject);
		}

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Pop-it",
			price: 7,
			category: (ShopItemCategory)ExplosivesCategory!,
			iconName: "popit_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterSilverFulminate()
	{
		// Build shaM_Jester_4 explosion prefab once for all grenades/semtex that use base GrenadeItemBehaviour

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SilverFulminateCristal.prefab");
		var visualRoot = prefab.transform.Find("Item/LargeFulminatedMercuryCristal")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterial.M_Child_1, true);

		var behaviour = prefab.AddComponent<GrenadeItemBehaviour>();
		behaviour.explodesOnImpact = true;

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 20f, radius: 12f, damage: 300f, force: 15f);
		AddGamefeel silverGamefeel = behaviour.explosionPrefab.GetComponent<AddGamefeel>();
		if (silverGamefeel != null)
		{
			silverGamefeel.perlinAmount = 50f;
			silverGamefeel.perlinDuration = 2f;
			silverGamefeel.scale = 25f;
			silverGamefeel.range = 100f;
		}
		ParticleSystem explosionParticle = behaviour.explosionPrefab.GetComponentInChildren<ParticleSystem>(true);
		ParticleSystem.EmissionModule emission = explosionParticle.emission;
		emission.SetBurst(0, new ParticleSystem.Burst(0f, 90f));
		var main = explosionParticle.main;
		main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 30f);
		main.startSize = new ParticleSystem.MinMaxCurve(10f, 15f);

		SFX_PlayOneShot spos = behaviour.explosionPrefab.GetComponent<SFX_PlayOneShot>();
		spos.sfx = _bundle!.LoadAsset<SFX_Instance>("SilverFulminateExplosionSfx");
		spos.sfxs = new SFX_Instance[] { };

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Silver fulminate crystal",
			price: 150,
			category: (ShopItemCategory)ExplosivesCategory!,
			iconName: "fulminate_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterUnbreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Unbreakable Bat.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Balaclava);
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
			iconName: "bat_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterBreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Breakable Bat.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Milk2);
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
			iconName: "bat_icon",
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
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterial.M_Balaclava, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cap")!.gameObject, GameMaterial.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rims")!.gameObject, GameMaterial.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Text")!.gameObject, GameMaterial.M_Cap_1, false);
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
			iconName: "o2tank_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterLargeO2Tank(SFX_Instance oxygenRegenerationSfx)
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.ContainerBounce1, ImpactSoundType.ContainerBounce2);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "LargeO2Tank.prefab");
		GameObject visualRoot = prefab.transform.Find("Visual/o2tank")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterial.M_Balaclava, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cap")!.gameObject, GameMaterial.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rims")!.gameObject, GameMaterial.M_Metal, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Text")!.gameObject, GameMaterial.M_Cap_1, false);

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
			iconName: "o2tank_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterBandAidBox()
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce3);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BandAidBox.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/bandaid")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Barcode")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cross")!.gameObject, GameMaterial.M_Jester_4, false);
		Renderer boxRenderer = visualRoot!.transform.Find("Box")!.gameObject.GetComponent<Renderer>();
		boxRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Milk2), GameMaterials.GetMaterial(GameMaterial.M_Child_1), GameMaterials.GetMaterial(GameMaterial.M_Jester_3) };
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Gauze")!.gameObject, GameMaterial.M_Child_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("PlasterFront")!.gameObject, GameMaterial.M_Hotdog_2, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TextBack")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TextFront")!.gameObject, GameMaterial.M_Balaclava, false);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 40;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Band-aid box",
			price: 30,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "bandaid_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterMedkit()
	{
		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce3);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Medkit.prefab");
		GameObject visualRoot = prefab.transform.Find("Visual/medkit")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Rim")!.gameObject, GameMaterial.M_Cap_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Case")!.gameObject, GameMaterial.M_Jester_4, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Cross")!.gameObject, GameMaterial.M_Cap_1, false);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 100;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Medkit",
			price: 70,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "medkit_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f),
			holdRot: new Vector3(0, 0, 0)
		);
	}

	private static void RegisterEnergyBar()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "EnergyBar.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/energybar")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!, GameMaterial.M_Balaclava, true);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Bar")!.gameObject, GameMaterial.M_Beanie_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Icon")!.gameObject, GameMaterial.M_Cap_1, false);
		var behaviour = prefab.AddComponent<TemporaryPlayerBoostItemBehaviour>();
		behaviour.playerCrunchSFX = _bundle!.LoadAsset<SFX_Instance>("CrunchSfx")!;


		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.BombBounce2);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Energy bar",
			price: 35,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "energybar_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterInvisibilitySpray()
	{
		var invisibilitySpraySfx = _bundle!.LoadAsset<SFX_Instance>("SpraySfx")!;
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "InvisibilitySpray.prefab");
		GameObject visualRoot = prefab.transform.Find("Item/spraycan")!.gameObject;
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Balaclava, true);
		GameObject cap = visualRoot!.transform.Find("Cap")!.gameObject;
		Renderer capRenderer = cap.GetComponent<Renderer>();
		capRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Cap_1), GameMaterials.GetMaterial(GameMaterial.M_Balaclava) };
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Icon")!.gameObject, GameMaterial.M_Jester_3, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("CanBridge")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("CapBridge")!.gameObject, GameMaterial.M_Cap_1, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Bulb")!.gameObject, GameMaterial.M_Cap_1, false);
		var behaviour = prefab.AddComponent<TemporaryInvisibilityItemBehaviour>();
		behaviour.invisibilitySpraySfx = invisibilitySpraySfx;


		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce6);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Invisibility spray",
			price: 65,
			category: (ShopItemCategory)ConsumablesCategory!,
			iconName: "invisibilityspray_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.7f)
		);
	}

	private static void RegisterGrenade()
	{
		// Build shaM_Jester_4 explosion prefab once for all grenades/semtex that use base GrenadeItemBehaviour

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Grenade.prefab");
		var visualRoot = prefab.transform.Find("Item/grenade")!.gameObject;
		var grenadeTransform = visualRoot.transform.Find("Grenade")!.gameObject;
		Renderer grenadeRenderer = grenadeTransform.GetComponent<Renderer>();
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Balaclava), GameMaterials.GetMaterial(GameMaterial.M_Child_2) };

		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("SmallTriggerRing")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TriggerRing")!.gameObject, GameMaterial.M_Balaclava, false);
		var behaviour = prefab.AddComponent<GrenadeItemBehaviour>();

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate();


		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Grenade",
			price: 40,
			category: (ShopItemCategory)ExplosivesCategory!,
			iconName: "grenade_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterSemtexGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SemtexGrenade.prefab");
		var visualRoot = prefab.transform.Find("Item/semtex")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Nails")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("SmallTriggerRing")!.gameObject, GameMaterial.M_Balaclava, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("TriggerRing")!.gameObject, GameMaterial.M_Balaclava, false);

		var grenadeRenderer = visualRoot.transform.Find("Grenade")!.gameObject.GetComponent<Renderer>();
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Child_2), GameMaterials.GetMaterial(GameMaterial.M_Jester_3), GameMaterials.GetMaterial(GameMaterial.M_Balaclava), GameMaterials.GetMaterial(GameMaterial.M_Child_1) };

		var behaviour = prefab.AddComponent<SemtexItemBehaviour>();

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate();

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
			price: 55,
			category: (ShopItemCategory)ExplosivesCategory!,
			iconName: "semtex_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}

	private static void RegisterElectricGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricGrenade.prefab");
		var visualRoot = prefab.transform.Find("Item/electric-grenade")!.gameObject;
		var grenadeRenderer = visualRoot.transform.Find("Grenade")!.gameObject.GetComponent<Renderer>();

		var templateExplosion = GetMonsterAffectingExplosionTemplate();
		var lightPointGameObject = templateExplosion.transform.Find("Point Light")!.gameObject;
		// gray, luminous M_Child_1, dark gray
		grenadeRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Player_1), GameMaterials.GetMaterial(GameMaterial.M_Child_1), GameMaterials.GetMaterial(GameMaterial.M_Balaclava) };

		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("InnerRods")!.gameObject, GameMaterial.M_Child_1, false);

		var partLRenderer = visualRoot.transform.Find("Part_L")!.gameObject.GetComponent<Renderer>();
		var partRRenderer = visualRoot.transform.Find("Part_R")!.gameObject.GetComponent<Renderer>();
		// for partL : darkgray, light, gray, flatgray
		partLRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Balaclava), GameMaterials.GetMaterial(GameMaterial.M_Child_1), GameMaterials.GetMaterial(GameMaterial.M_Player_1), GameMaterials.GetMaterial(GameMaterial.M_Player) };
		// for partR : darkgray, gray, flatgray, light
		partRRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Balaclava), GameMaterials.GetMaterial(GameMaterial.M_Player_1), GameMaterials.GetMaterial(GameMaterial.M_Player), GameMaterials.GetMaterial(GameMaterial.M_Child_1) };

		var explosionPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricExplosion.prefab");
		var lightPoint = UnityEngine.Object.Instantiate(lightPointGameObject, explosionPrefab.transform);
		lightPoint.GetComponent<Light>().color = Color.blue;
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
			price: 90,
			category: (ShopItemCategory)ExplosivesCategory!,
			iconName: "electricgrenade_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.6f, 0.7f)
		);
	}


	private static void RegisterJumpingBoots()
	{
		FroggyBootRightPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootRight.prefab");
		GameMaterials.ApplyMaterial(FroggyBootRightPrefab, GameMaterial.M_Child_2, true);
		FroggyBootLeftPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootLeft.prefab");
		GameMaterials.ApplyMaterial(FroggyBootLeftPrefab, GameMaterial.M_Child_2, true);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BootsEquipable.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Child_2);
		prefab.AddComponent<BootsEquipableItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		JumpingBootsItem = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Froggy boots",
			price: 100,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "froggyboots_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	private static void RegisterCursedDoll()
	{
		CursedNecklacePrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "CursedNecklace.prefab");
		GameMaterials.ApplyMaterial(CursedNecklacePrefab, GameMaterial.M_Child_2, true);

		var visualRoot = CursedNecklacePrefab.transform.Find("CursedNecklace")!.gameObject;
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Torus")!.gameObject, GameMaterial.M_Player, false);
		GameMaterials.ApplyMaterial(visualRoot!.transform.Find("Mesh_0")!.gameObject, GameMaterial.M_Child_5, false);

		var dollRenderer = visualRoot.transform.Find("Mesh_0")!.gameObject.GetComponent<Renderer>();
		dollRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Child_5) };
		var necklaceRenderer = visualRoot.transform.Find("Torus")!.gameObject.GetComponent<Renderer>();
		necklaceRenderer.materials = new Material[] { GameMaterials.GetMaterial(GameMaterial.M_Player) };

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "DollItem.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Child_5, true);

		AudioClip squink = _bundle!.LoadAsset<AudioClip>("Squink");
		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(squink);

		prefab.AddComponent<DollEquipableBehaviour>();

		CursedDoll = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Cursed Doll",
			price: 80,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "doll_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	private static void RegisterAngelWings()
	{
		AngelWingsPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "WingsVisual.prefab");
		GameMaterials.ApplyMaterial(AngelWingsPrefab, GameMaterial.M_Child_1, true);
		AngelWingsPrefab.AddComponent<AngelWingsVisualAnimationHandler>();

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "WingsItem.prefab");
		GameMaterials.ApplyMaterial(prefab, GameMaterial.M_Child_1);
		prefab.AddComponent<AngelWingsEquipableItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		AngelWingsItem = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Angel wings",
			price: 100,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "wings_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	private static void RegisterGlowingVest()
	{
		// SmallLightBeamPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "VestLightPrefab.prefab");
		var randomLamp = Items.GetItemByPrefabComponent<Flashlight>();
		if (randomLamp != null)
		{
			HierarchyUtility.LogHierarchy(randomLamp.itemObject);
			var tempInstance = GameObject.Instantiate(randomLamp.itemObject);
			var lightObject = tempInstance.transform.Find("Light")!.gameObject;
			lightObject.transform.SetParent(null);
			UnityEngine.Object.DestroyImmediate(tempInstance);
			lightObject.SetActive(false);
			var lightComponent = lightObject.GetComponent<Light>();
			lightComponent.innerSpotAngle = 0f;
			lightComponent.intensity = 60f;
			lightComponent.spotAngle = 40f;
			lightComponent.enabled = true;
			SmallLightBeamPrefab = lightObject;
			UnityEngine.Object.DontDestroyOnLoad(lightObject);
		}

		GlowingVestPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "VestVisual.prefab");
		var materials = new Material[] {
			GameMaterials.GetMaterial(GameMaterial.M_Balaclava),
			GameMaterials.GetMaterial(GameMaterial.M_Flashlight_1_1),
			GameMaterials.GetMaterial(GameMaterial.M_Child_1),
			GameMaterials.GetMaterial(GameMaterial.M_Jester_3),
			GameMaterials.GetMaterial(GameMaterial.M_FredGull_2)
		};

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "VestItem.prefab");

		{
			var visualRoot = prefab.transform.Find("Item/vest")!.gameObject;
			var renderer = visualRoot.transform.Find("Vest")!.gameObject.GetComponent<Renderer>();
			renderer.materials = materials;
		}
		{
			var renderer = GlowingVestPrefab.transform.Find("Vest")!.gameObject.GetComponent<Renderer>();
			renderer.materials = materials;
		}
		prefab.AddComponent<GlowingVestEquipableItemBehaviour>();

		SFX_Instance[] impactSounds = ImpactSoundScanner.GetImpactSounds(ImpactSoundType.PlasticBounce1);

		GlowingVest = Items.RegisterItem(
			bundle: _bundle!,
			prefab: prefab,
			displayName: "Bright vest",
			price: 150,
			category: (ShopItemCategory)EquipablesCategory!,
			iconName: "glowing_vest_icon",
			impactSounds: impactSounds,
			holdPos: new Vector3(0.3f, -0.3f, 0.4f)
		);
	}

	public static Sprite GetSprite(string iconName)
	{
		return _bundle!.LoadAsset<Sprite>(iconName);
	}
}