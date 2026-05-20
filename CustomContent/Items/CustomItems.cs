using UnityEngine;
using DbsContentApi;
using Photon.Pun;
using UnlistedEntities.CustomContent.ContentEvents;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Handles the setup and registration of specific custom items for this mod.
/// Uses the generic Items API.
/// </summary>
public static class CustomItems
{
	private static AssetBundle? _bundle;
	public static ShopItemCategory WeaponsCategory;
	public static ShopItemCategory ConsumablesCategory;
	public static ShopItemCategory EquipablesCategory;
	public static ShopItemCategory ExplosivesCategory;

	public static GameObject? FroggyBootRightPrefab = null;
	public static GameObject? FroggyBootLeftPrefab = null;
	public static GameObject? AngelWingsPrefab = null;
	public static GameObject? CursedNecklaceVisualPrefab = null;
	public static GameObject? GlowingVestPrefab = null;
	public static GameObject? SmallLightBeamPrefab = null;
	public static Item? JumpingBootsItem = null;
	public static Item? CursedDoll = null;
	public static Item? AngelWingsItem = null;
	public static Item? GlowingVest = null;

	private static GameObject GetMonsterAffectingExplosionTemplate(float fall = 3f, float radius = 8f, float innerRadius = 4f, float damage = 150f, float force = 4f)
	{
		Item? bombItem = Items.GetItemByPrefabComponent<BombItem>();
		if (bombItem == null || bombItem.itemObject == null)
		{
			Logger.Log("CustomItems: Could not find BombItem to copy explosion from!");
			return null!;
		}

		BombItem? bombItemBehaviour = bombItem.itemObject.GetComponent<BombItem>();
		if (bombItemBehaviour == null || bombItemBehaviour.explosion == null)
		{
			Logger.Log("CustomItems: BombItem has no explosion prefab!");
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
		newAoe.innerRadius = innerRadius;
		newAoe.damage = damage;
		newAoe.force = force;

		// Add PhotonView for network synchronization
		var pv = explosionInstance.AddComponent<PhotonView>();
		pv.Synchronization = ViewSynchronization.Off; // Visuals don't need continuous sync
		pv.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

		return explosionInstance;
	}

	/// <summary>
	/// Configures all custom items using the loaded AssetBundle.
	/// </summary>
	/// <param name="bundle">The AssetBundle containing item assets.</param>
	public static void Setup(AssetBundle bundle)
	{
		_bundle = bundle;

		WeaponsCategory = Items.RegisterCustomCategory("Weapons");
		ConsumablesCategory = Items.RegisterCustomCategory("Consumables");
		EquipablesCategory = Items.RegisterCustomCategory("Equipables");
		ExplosivesCategory = Items.RegisterCustomCategory("Explosives");

		// Register content events — order is fixed and must not change (IDs are index-based)
		DbsContentApi.ContentEvents.RegisterEvent(new PopitExplosionContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new GrenadeExplosionContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new SemtexExplosionContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new SilverFulminateExplosionContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new BatHitMonsterContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new SemtexStickMonsterContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new ElectricGrenadeMonsterContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new SilverFulminateMonsterContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new TranqGunEnemyContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new BatHitAllyContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new InvisiblePlayerContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new SemtexStickAllyContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new ElectricGrenadeAllyContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new GlowingVestContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new AngelWingsContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new CursedDollContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new FroggyBootsContentEvent());
		DbsContentApi.ContentEvents.RegisterEvent(new TranqGunAllyContentEvent());

		var oxygenRegenerationSfx = _bundle!.LoadAsset<SFX_Instance>("OxygenRegenSfx")!;

		Items.DeferRegistration(() =>
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
		});
	}

	private static void RegisterTranqGun()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "TranqGun2.prefab");
		GameMaterials.Batch(prefab)
			.At("TranqGun/Barrel", GameMaterial.M_Metal)
			.At("TranqGun/Lower", GameMaterial.M_Player)
			.At("TranqGun/Magazine", GameMaterial.M_Balaclava)
			.At("TranqGun/Rear_Sights", GameMaterial.M_Metal)
			.At("TranqGun/Red_Dot_Sight", GameMaterial.M_Metal)
			.At("TranqGun/Slide", GameMaterial.M_Balaclava)
			.At("TranqGun/Trigger", GameMaterial.M_Metal);
		// Configure TranqGun behaviour
		var behaviour = prefab.AddComponent<TranqGunItemBehaviour>();
		behaviour.firePoint = prefab.transform.Find("FirePoint");

		SFX_Instance shotSfx = _bundle!.LoadAsset<SFX_Instance>("TranqGunShotSfx");

		// Load and configure projectile prefab
		GameObject shotPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "TranqShot.prefab");
		GameMaterials.Batch(shotPrefab)
			.At("wrapper/glass", GameMaterial.M_ShopGlass)
			.At("wrapper/liquid", GameMaterial.M_Child_2)
			.At("wrapper/shell", GameMaterial.M_Metal);

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
		proj.fall = 0f;
		proj.damage = 0f; // Tranq gun doesn't deal direct damage
		proj.force = 0f;
		proj.postHitBehavior = Projectile.PostHitBehaviour.Disable;
		proj.layerType = HelperFunctions.LayerType.All;

		shotPrefab.AddComponent<TranqProjectileLogic>();

		var stickComponent = shotPrefab.AddComponent<ProjectileStick>();
		var syringeHitSfx = _bundle!.LoadAsset<SFX_Instance>("SyringeHitSfx");
		if (syringeHitSfx == null)
		{
			Logger.LogError("CustomItems: SyringeHitSfx not found!");
		}
		else
		{
			stickComponent.hitSFX = new SFX_Instance[] { syringeHitSfx };
		}

		var remove = shotPrefab.AddComponent<RemoveAfterSeconds>();
		remove.seconds = 10f;

		behaviour.projectilePrefab = shotPrefab;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Tranquilizer gun",
			price = 200,
			category = WeaponsCategory,
			icon = GetSprite("tranqgun_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f),
			holdRot = new Vector3(0, 0, 0),
			useAlternativeHoldPos = true,
			useAlternativeHoldRot = false,
			alternativeHoldPos = new Vector3(0.2f, -0.22f, 0.7f)
		});
	}

	private static void RegisterPopit()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Popit.prefab");
		GameMaterials.Batch(prefab).At("Item/PopIt", DescriptiveMaterial.WHITE_1, deep: true);

		var behaviour = prefab.AddComponent<PopitItemBehaviour>();
		behaviour.explodesOnImpact = true;

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 1.5f, radius: 3.5f, innerRadius: 2f, damage: 10f, force: 2.5f);
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
			UnityEngine.Object.Destroy(explosionLight.gameObject);
		}

		behaviour.explosionPrefab.name = "Explosion_PopIt";

		ContentLoader.RegisterPrefabInPhotonPool(behaviour.explosionPrefab);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Pop-it",
			price = 7,
			category = ExplosivesCategory,
			icon = GetSprite("popit_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.6f, 0.7f)
		});
	}

	private static void RegisterSilverFulminate()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SilverFulminateCristal.prefab");
		GameMaterials.Batch(prefab).At("Item/LargeFulminatedMercuryCristal", DescriptiveMaterial.WHITE_3, deep: true);

		var behaviour = prefab.AddComponent<SilverFulminateItemBehaviour>();
		behaviour.explodesOnImpact = true;

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 20f, radius: 12f, innerRadius: 6f, damage: 300f, force: 15f);
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

		behaviour.explosionPrefab.name = "Explosion_SilverFulminate";

		var silverAoe = behaviour.explosionPrefab.GetComponent<MonsterAffectingAOE>();
		if (silverAoe == null)
		{
			Logger.LogError("CustomItems.RegisterSilverFulminate: MonsterAffectingAOE missing on explosion prefab; monster-hit content callbacks will not run.");
		}
		else if (DbsContentApiPlugin.TemporaryContentTriggerPrefab == null)
		{
			Logger.LogError("CustomItems.RegisterSilverFulminate: TemporaryContentTriggerPrefab is null; skipping SilverFulminateMonsterContentProvider monster-hit wiring.");
		}
		else
		{
			silverAoe.onMonsterHit = (monster, pos) =>
			{
				if (DbsContentApiPlugin.TemporaryContentTriggerPrefab == null)
				{
					Logger.LogError("SilverFulminate onMonsterHit: TemporaryContentTriggerPrefab became null; cannot spawn SilverFulminateMonsterContentProvider.");
					return;
				}

				if (monster?.refs?.view == null)
				{
					Logger.LogError("SilverFulminate onMonsterHit: monster Player has null refs/view; SilverFulminateMonsterContentProvider not spawned.");
					return;
				}

				GameObject trigger = ObjectHelper.CreateTemporaryTriggerObject(50, DbsContentApiPlugin.TemporaryContentTriggerPrefab);
				if (trigger == null)
				{
					Logger.LogError("SilverFulminate onMonsterHit: CreateTemporaryTriggerObject returned null.");
					return;
				}

				trigger.transform.position = pos;
				trigger.AddComponent<SilverFulminateMonsterContentProvider>();
			};
		}

		ContentLoader.RegisterPrefabInPhotonPool(behaviour.explosionPrefab);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Silver fulminate crystal",
			price = 150,
			category = ExplosivesCategory,
			icon = GetSprite("fulminate_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.6f, 0.7f)
		});
	}

	private static void RegisterUnbreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Unbreakable Bat.prefab");
		GameMaterials.Batch(prefab).Root(GameMaterial.M_Balaclava);
		BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
		batBehaviour.batHitSFX = _bundle!.LoadAsset<SFX_Instance>("SFX Bat Hit");
		batBehaviour.isBreakable = false;

		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(_bundle!.LoadAsset<AudioClip>("bat_fall"));
		foreach (SFX_Instance sfx in impactSounds)
		{
			sfx.settings.pitch = 0.8f;
			sfx.settings.volume = 0.7f;
		}

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Solid bat",
			price = 600,
			category = WeaponsCategory,
			icon = GetSprite("bat_icon"),
			impactSounds = impactSounds,
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterBreakableBat()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Breakable Bat.prefab");
		GameMaterials.Batch(prefab).Root(DescriptiveMaterial.BROWN_1);
		BatBehaviour batBehaviour = prefab.AddComponent<BatBehaviour>();
		batBehaviour.batHitSFX = _bundle!.LoadAsset<SFX_Instance>("SFX Bat Hit");
		batBehaviour.isBreakable = true;

		AudioClip batFallClip = _bundle!.LoadAsset<AudioClip>("bat_fall");
		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(batFallClip);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Fragile bat",
			price = 40,
			category = WeaponsCategory,
			icon = GetSprite("bat_icon"),
			impactSounds = impactSounds,
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterSmallO2Tank(SFX_Instance oxygenRegenerationSfx)
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SmallO2Tank.prefab");
		ApplyO2TankMaterials(prefab);
		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 30;
		behaviour.statType = StatTypeEnum.Oxygen;
		behaviour.oxygenRegenerationSfx = oxygenRegenerationSfx;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Small O2 tank",
			price = 30,
			category = ConsumablesCategory,
			icon = GetSprite("o2tank_icon"),
			impactSoundTypes = new[] { ImpactSoundType.ContainerBounce1, ImpactSoundType.ContainerBounce2 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterLargeO2Tank(SFX_Instance oxygenRegenerationSfx)
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "LargeO2Tank.prefab");
		ApplyO2TankMaterials(prefab);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 80;
		behaviour.statType = StatTypeEnum.Oxygen;
		behaviour.oxygenRegenerationSfx = oxygenRegenerationSfx;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Large O2 tank",
			price = 80,
			category = ConsumablesCategory,
			icon = GetSprite("o2tank_icon"),
			impactSoundTypes = new[] { ImpactSoundType.ContainerBounce1, ImpactSoundType.ContainerBounce2 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterBandAidBox()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BandAidBox.prefab");
		GameMaterials.Batch(prefab)
			.At("Item/bandaid/Barcode", GameMaterial.M_Balaclava)
			.At("Item/bandaid/Cross", DescriptiveMaterial.RED_2)
			.At("Item/bandaid/Gauze", DescriptiveMaterial.WHITE_1)
			.At("Item/bandaid/PlasterFront", DescriptiveMaterial.BEIGE_1)
			.At("Item/bandaid/TextBack", GameMaterial.M_Balaclava)
			.At("Item/bandaid/TextFront", GameMaterial.M_Balaclava)
			.AtSlot("Item/bandaid/Box", 0, GameMaterial.M_Milk2)
			.AtSlot("Item/bandaid/Box", 1, DescriptiveMaterial.WHITE_1)
			.AtSlot("Item/bandaid/Box", 2, DescriptiveMaterial.BLUE_1);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 40;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Band-aid box",
			price = 20,
			category = ConsumablesCategory,
			icon = GetSprite("bandaid_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce3 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterMedkit()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Medkit.prefab");
		GameMaterials.Batch(prefab)
			.At("Visual/medkit/Rim", GameMaterial.M_Cap_1)
			.At("Visual/medkit/Case", DescriptiveMaterial.RED_2)
			.At("Visual/medkit/Cross", GameMaterial.M_Cap_1);

		RegenStatItemBehaviour behaviour = prefab.AddComponent<RegenStatItemBehaviour>();
		behaviour.regenPercentageAmount = 100;
		behaviour.statType = StatTypeEnum.Health;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Medkit",
			price = 50,
			category = ConsumablesCategory,
			icon = GetSprite("medkit_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce3 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f),
			holdRot = new Vector3(0, 0, 0)
		});
	}

	private static void RegisterEnergyBar()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "EnergyBar.prefab");
		GameMaterials.Batch(prefab)
			.At("Item/energybar", GameMaterial.M_Balaclava, deep: true)
			.At("Item/energybar/Bar", GameMaterial.M_Beanie_1)
			.At("Item/energybar/Icon", GameMaterial.M_Cap_1);
		var behaviour = prefab.AddComponent<TemporaryPlayerBoostItemBehaviour>();
		behaviour.playerCrunchSFX = _bundle!.LoadAsset<SFX_Instance>("CrunchSfx")!;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Energy bar",
			price = 25,
			category = ConsumablesCategory,
			icon = GetSprite("energybar_icon"),
			impactSoundTypes = new[] { ImpactSoundType.BombBounce2 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterInvisibilitySpray()
	{
		var invisibilitySpraySfx = _bundle!.LoadAsset<SFX_Instance>("SpraySfx")!;
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "InvisibilitySpray.prefab");
		GameMaterials.Batch(prefab)
			.Root(GameMaterial.M_Balaclava, deep: true)
			.Slots("Item/spraycan/Cap", GameMaterial.M_Cap_1, GameMaterial.M_Balaclava)
			.At("Item/spraycan/Icon", DescriptiveMaterial.YELLOW_1)
			.At("Item/spraycan/CanBridge", GameMaterial.M_Balaclava)
			.At("Item/spraycan/CapBridge", GameMaterial.M_Cap_1)
			.At("Item/spraycan/Bulb", GameMaterial.M_Cap_1);
		var behaviour = prefab.AddComponent<TemporaryInvisibilityItemBehaviour>();
		behaviour.invisibilitySpraySfx = invisibilitySpraySfx;

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Invisibility spray",
			price = 45,
			category = ConsumablesCategory,
			icon = GetSprite("invisibilityspray_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce6 },
			holdPos = new Vector3(0.3f, -0.3f, 0.7f)
		});
	}

	private static void RegisterGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "Grenade.prefab");
		GameMaterials.Batch(prefab)
			.Slots("Item/grenade/Grenade", GameMaterial.M_Balaclava, GameMaterial.M_Child_2)
			.At("Item/grenade/SmallTriggerRing", GameMaterial.M_Balaclava)
			.At("Item/grenade/TriggerRing", GameMaterial.M_Balaclava);
		var behaviour = prefab.AddComponent<GrenadeItemBehaviour>();

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 20f, radius: 8f, damage: 150f, force: 15f);
		behaviour.explosionPrefab.name = "Explosion_Grenade";

		ContentLoader.RegisterPrefabInPhotonPool(behaviour.explosionPrefab);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Grenade",
			price = 30,
			category = ExplosivesCategory,
			icon = GetSprite("grenade_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.6f, 0.7f)
		});
	}

	private static void RegisterSemtexGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "SemtexGrenade.prefab");
		GameMaterials.Batch(prefab)
			.At("Item/semtex/Nails", GameMaterial.M_Balaclava)
			.At("Item/semtex/SmallTriggerRing", GameMaterial.M_Balaclava)
			.At("Item/semtex/TriggerRing", GameMaterial.M_Balaclava)
			.AtSlot("Item/semtex/Grenade", 0, GameMaterial.M_Child_2)
			.AtSlot("Item/semtex/Grenade", 1, GameMaterial.M_Jester_3)
			.AtSlot("Item/semtex/Grenade", 2, GameMaterial.M_Balaclava)
			.AtSlot("Item/semtex/Grenade", 3, DescriptiveMaterial.WHITE_1);

		var behaviour = prefab.AddComponent<SemtexItemBehaviour>();

		behaviour.explosionPrefab = GetMonsterAffectingExplosionTemplate(fall: 2f, radius: 8f, innerRadius: 4f, damage: 150f, force: 4f);

		behaviour.hasTickingSound = true;
		behaviour.onStickSfx = _bundle!.LoadAsset<SFX_Instance>("SemtexStickSfx");
		behaviour.tickingSoundClip = _bundle!.LoadAsset<AudioClip>("semtex-tick");
		behaviour.baseTickingPitch = 0.8f;
		behaviour.tickingPitchMultiplier = 0.25f;
		behaviour.maxTickingPitch = 8f;
		behaviour.minTickingPitch = 1f;
		behaviour.tickingVolume = 0.35f;

		behaviour.explosionPrefab.name = "Explosion_SemtexGrenade";

		ContentLoader.RegisterPrefabInPhotonPool(behaviour.explosionPrefab);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Semtex grenade",
			price = 50,
			category = ExplosivesCategory,
			icon = GetSprite("semtex_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.6f, 0.7f)
		});
	}

	private static void RegisterElectricGrenade()
	{
		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricGrenade.prefab");
		ApplyElectricGrenadeMaterials(prefab);

		var templateExplosion = GetMonsterAffectingExplosionTemplate();
		var lightPointGameObject = templateExplosion.transform.Find("Point Light")!.gameObject;

		var explosionPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "ElectricExplosion.prefab");
		var lightPoint = UnityEngine.Object.Instantiate(lightPointGameObject, explosionPrefab.transform);
		lightPoint.GetComponent<Light>().color = Color.blue;
		explosionPrefab.AddComponent<ElectricGrenadeExplosionAOE>();

		// Add PhotonView for network synchronization
		var pv = explosionPrefab.AddComponent<PhotonView>();
		pv.Synchronization = ViewSynchronization.Off;
		pv.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

		var behaviour = prefab.AddComponent<ElectricGrenadeItemBehaviour>();
		behaviour.baseTickingPitch = 0.8f;
		behaviour.tickingPitchMultiplier = 0.25f;
		behaviour.maxTickingPitch = 8f;
		behaviour.minTickingPitch = 1f;
		behaviour.hasTickingSound = true;
		behaviour.explosionPrefab = explosionPrefab;
		behaviour.tickingSoundClip = _bundle!.LoadAsset<AudioClip>("electric-grenade-tick");
		behaviour.tickingVolume = 0.35f;

		explosionPrefab.name = "Explosion_ElectricGrenade";

		ContentLoader.RegisterPrefabInPhotonPool(explosionPrefab);

		Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Electric grenade",
			price = 90,
			category = ExplosivesCategory,
			icon = GetSprite("electricgrenade_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.6f, 0.7f)
		});
	}


	private static void RegisterJumpingBoots()
	{
		FroggyBootRightPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootRight.prefab");
		GameMaterials.Batch(FroggyBootRightPrefab).Root(GameMaterial.M_Child_2, deep: true);
		FroggyBootLeftPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "FroggyBootLeft.prefab");
		GameMaterials.Batch(FroggyBootLeftPrefab).Root(GameMaterial.M_Child_2, deep: true);

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "BootsEquipable.prefab");
		GameMaterials.Batch(prefab).Root(GameMaterial.M_Child_2);
		prefab.AddComponent<BootsEquipableItemBehaviour>();

		JumpingBootsItem = Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Froggy boots",
			price = 100,
			category = EquipablesCategory,
			icon = GetSprite("froggyboots_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.3f, 0.4f)
		});
	}

	private static void RegisterCursedDoll()
	{
		CursedNecklaceVisualPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "CursedNecklace.prefab");
		GameMaterials.Batch(CursedNecklaceVisualPrefab)
			.Root(GameMaterial.M_Child_2, deep: true)
			.Slots("CursedNecklace/Mesh_0", DescriptiveMaterial.BROWN_1)
			.Slots("CursedNecklace/Torus", GameMaterial.M_Player);

		GameObject dollItemPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "DollItem.prefab");
		GameMaterials.Batch(dollItemPrefab).Root(DescriptiveMaterial.BROWN_1, deep: true);

		AudioClip squink = _bundle!.LoadAsset<AudioClip>("Squink");
		SFX_Instance[] impactSounds = Items.CreateSFXInstanceFromClip(squink);

		dollItemPrefab.AddComponent<DollEquipableBehaviour>();

		CursedDoll = Items.RegisterItem(dollItemPrefab, new ItemConfig
		{
			displayName = "Cursed Doll",
			price = 170,
			category = EquipablesCategory,
			icon = GetSprite("doll_icon"),
			impactSounds = impactSounds,
			holdPos = new Vector3(0.3f, -0.3f, 0.4f)
		});
	}

	private static void RegisterAngelWings()
	{
		AngelWingsPrefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "WingsVisual.prefab");
		GameMaterials.Batch(AngelWingsPrefab).Root(DescriptiveMaterial.WHITE_1, deep: true);
		AngelWingsPrefab.AddComponent<AngelWingsVisualAnimationHandler>();

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "WingsItem.prefab");
		GameMaterials.Batch(prefab).Root(DescriptiveMaterial.WHITE_1);
		prefab.AddComponent<AngelWingsEquipableItemBehaviour>();

		AngelWingsItem = Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Angel wings",
			price = 100,
			category = EquipablesCategory,
			icon = GetSprite("wings_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.3f, 0.4f)
		});
	}

	private static void RegisterGlowingVest()
	{
		var randomLamp = Items.GetItemByPrefabComponent<Flashlight>();
		if (randomLamp != null)
		{
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

		GameObject prefab = ContentLoader.LoadPrefabFromBundle(_bundle!, "VestItem.prefab");

		ApplyGlowingVestMaterials(prefab);
		ApplyGlowingVestMaterials(GlowingVestPrefab);
		prefab.AddComponent<GlowingVestEquipableItemBehaviour>();

		GlowingVest = Items.RegisterItem(prefab, new ItemConfig
		{
			displayName = "Bright vest",
			price = 200,
			category = EquipablesCategory,
			icon = GetSprite("glowing_vest_icon"),
			impactSoundTypes = new[] { ImpactSoundType.PlasticBounce1 },
			holdPos = new Vector3(0.3f, -0.3f, 0.4f)
		});
	}

	public static Sprite GetSprite(string iconName)
	{
		return _bundle!.LoadAsset<Sprite>(iconName);
	}

	private static void ApplyO2TankMaterials(GameObject prefab)
	{
		GameMaterials.Batch(prefab)
			.At("Visual/o2tank", GameMaterial.M_Balaclava, deep: true)
			.At("Visual/o2tank/Cap", DescriptiveMaterial.RED_1)
			.At("Visual/o2tank/Rims", DescriptiveMaterial.GREY_SMALL_DARK_SPOTS)
			.At("Visual/o2tank/Text", DescriptiveMaterial.WHITE_1);
	}

	private static void ApplyElectricGrenadeMaterials(GameObject prefab)
	{
		const string root = "Item/electric-grenade";
		GameMaterials.Batch(prefab)
			.AtSlot($"{root}/Grenade", 0, GameMaterial.M_Player_1)
			.AtSlot($"{root}/Grenade", 1, DescriptiveMaterial.GLOWING_WHITE_2)
			.AtSlot($"{root}/Grenade", 2, GameMaterial.M_Balaclava)
			.At($"{root}/InnerRods", DescriptiveMaterial.GLOWING_WHITE_2)
			.AtSlot($"{root}/Part_L", 0, GameMaterial.M_Balaclava)
			.AtSlot($"{root}/Part_L", 1, DescriptiveMaterial.GLOWING_WHITE_2)
			.AtSlot($"{root}/Part_L", 2, GameMaterial.M_Player_1)
			.AtSlot($"{root}/Part_L", 3, GameMaterial.M_Player)
			.AtSlot($"{root}/Part_R", 0, GameMaterial.M_Balaclava)
			.AtSlot($"{root}/Part_R", 1, GameMaterial.M_Player_1)
			.AtSlot($"{root}/Part_R", 2, GameMaterial.M_Player)
			.AtSlot($"{root}/Part_R", 3, DescriptiveMaterial.GLOWING_WHITE_2);
	}

	private static void ApplyGlowingVestMaterials(GameObject prefab)
	{
		const string vestPath = "Item/vest/Vest";
		string path = prefab.transform.Find(vestPath) != null ? vestPath : "Vest";
		GameMaterials.Batch(prefab)
			.AtSlot(path, 0, GameMaterial.M_Balaclava)
			.AtSlot(path, 1, GameMaterial.M_Flashlight_1_1)
			.AtSlot(path, 2, DescriptiveMaterial.GLOWING_WHITE_2)
			.AtSlot(path, 3, DescriptiveMaterial.YELLOW_1)
			.AtSlot(path, 4, GameMaterial.M_Pool_7);
	}
}
