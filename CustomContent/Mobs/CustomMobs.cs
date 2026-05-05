using UnityEngine;
using DbsContentApi.Modules;
using Logger = DbsContentApi.Modules.Logger;
using DbsContentApi;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Handles the setup and registration of specific custom monsters for this mod.
/// Uses the generic Mobs API.
/// </summary>
public static class CustomMobs
{
    public static GameObject? MainCharacter { get; private set; }
    public static GameObject? TeapotFinal { get; private set; }
    public static GameObject? ReaperPrefab { get; private set; }
    public static GameObject? SurfaceNavmeshPrefab { get; private set; }

    /// <summary>
    /// Configures all custom monsters using the loaded AssetBundle.
    /// </summary>
    /// <param name="bundle">The AssetBundle containing monster prefabs.</param>
    public static void Setup(AssetBundle bundle)
    {
        Logger.Log("Starting custom mobs setup");

        Logger.Log("Loading TeapotFinal prefab from bundle");
        TeapotFinal = ContentLoader.LoadPrefabFromBundle(bundle, "TeapotFinal.prefab");

        Logger.Log("Loading Reaper prefab from bundle");
        ReaperPrefab = ContentLoader.LoadPrefabFromBundle(bundle, "Reaper.prefab");

        RegisterTeapot(bundle);
        // RegisterReaper(bundle);

        InitializeSurfaceNavmesh(bundle);

        Logger.Log("Custom mobs setup completed");
    }


    private static void InitializeSurfaceNavmesh(AssetBundle bundle)
    {
        Logger.Log("Initializing surface navmesh");
        SurfaceNavmeshPrefab = ContentLoader.LoadPrefabFromBundle(bundle, "Surface_Navmesh.prefab");
        if (SurfaceNavmeshPrefab == null)
        {
            Logger.LogError("Failed to load SurfaceNavmesh.prefab from bundle");
            return;
        }
    }

    private static void RegisterReaper(AssetBundle bundle)
    {
        if (ReaperPrefab == null)
        {
            Logger.LogError("Reaper prefab failed to load; skipping Reaper registration");
            return;
        }

        Logger.Log($"Registering Reaper: {ReaperPrefab.name}");

        Logger.Log("Creating MobSetupConfig for Reaper (using existing RigCreator setup)");
        var config = new MobSetupConfig
        {
            // visualRig = null means use the existing RigCreator + PlayerVisual configuration on the prefab
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 1, rarity = 1f },
            controller = new ControllerConfig { gravity = 20f, movementForce = 20f },
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig { monsterName = ReaperPrefab.name },
            navMesh = new NavMeshAgentConfig { height = 1.8f, radius = 0.64f, speed = 3.5f, wide = true },
            monsterAnimationValues = new MonsterAnimationValuesConfig { rightPunch = false, leftPunch = false },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        Logger.Log("Setting up Reaper custom monster");
        Mobs.SetupCustomMonster(ReaperPrefab, ReaperPrefab.name, config);

        Logger.Log("Getting Bot child object for Reaper");
        GameObject botReaper = Mobs.GetBotChildObject(ReaperPrefab);

        Logger.Log("Adding Bot_Chaser component to Reaper");
        Mobs.AddBotChaserComponent(botReaper);

        ReaperPrefab.AddComponent<ReaperContentProvider>();
        Logger.Log("ReaperContentProvider added to Reaper");

        DbsContentApi.Modules.ContentEvents.RegisterEvent(new ReaperContentEvent());

        Logger.Log("Adding Reaper to customMonsters list");
        DbsContentApiPlugin.customMonsters.Add(ReaperPrefab);
        Logger.Log($"Reaper registration completed: {ReaperPrefab.name}");

        GameMaterials.OnMaterialsLoaded += () =>
        {
            GameMaterials.ApplyMaterial(ReaperPrefab!, GameMaterial.M_Monster, true);
        };

    }
    private static void RegisterTeapot(AssetBundle bundle)
    {
        if (TeapotFinal == null)
        {
            Logger.LogError("TeapotFinal prefab failed to load; skipping Teapot registration");
            return;
        }

        Logger.Log($"Registering TeapotFinal: {TeapotFinal.name}");

        Logger.Log("Restoring shaders for TeapotFinal");
        // Mobs.RestoreShaders(TeapotFinal);

        Logger.Log("Creating MobSetupConfig for TeapotFinal (using existing RigCreator setup)");
        var config = new MobSetupConfig
        {
            // visualRig = null means use the existing RigCreator + PlayerVisual configuration on the prefab
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 1, rarity = 1f },
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig { monsterName = TeapotFinal.name },
            navMesh = new NavMeshAgentConfig { height = 2f, radius = 1f, speed = 3.5f, wide = true },
            monsterAnimationValues = new MonsterAnimationValuesConfig { rightPunch = false, leftPunch = false },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        Logger.Log("Setting up TeapotFinal custom monster");
        Mobs.SetupCustomMonster(TeapotFinal, TeapotFinal.name, config);

        Logger.Log("Getting Bot child object for TeapotFinal");
        GameObject botTeapot = Mobs.GetBotChildObject(TeapotFinal);

        Logger.Log("Adding Bot_Chaser component to TeapotFinal");
        Mobs.AddBotChaserComponent(botTeapot);

        Logger.Log("Adding Attack_Teapot component to TeapotFinal bot");
        var teapotAttack = botTeapot.AddComponent<Attack_Teapot>();
        // teapotAttack.enabled = false;
        var beakTransform = TeapotFinal.transform.Find("Visual/TeapotFinal/Armature/Hip/Spine_1/Head/Beak");
        teapotAttack.beakTransform = beakTransform;
        if (beakTransform != null)
        {
            Logger.Log("Beak transform found and assigned to Attack_Teapot");
        }
        if (beakTransform == null)
        {
            Logger.LogError("Beak transform not found for Attack_Teapot");
        }

        Logger.Log("Loading TeapotDroplet prefab from bundle");
        GameObject teapotDroplet = bundle.LoadAsset<GameObject>("TeapotDroplet.prefab");
        if (teapotDroplet != null)
        {
            Logger.Log("TeapotDroplet prefab loaded, configuring materials");
            teapotAttack.waterProjectilePrefab = teapotDroplet;
        }
        if (teapotDroplet == null)
        {
            Logger.LogError("TeapotDroplet.prefab not found in bundle");
        }

        Logger.Log("Loading SFX assets for TeapotFinal");
        teapotAttack.hardBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilHard.asset");
        if (teapotAttack.hardBoilingWaterSfx != null)
        {
            Logger.Log("BoilHard.asset loaded");
        }
        if (teapotAttack.hardBoilingWaterSfx == null)
        {
            Logger.LogError("BoilHard.asset not found in bundle");
        }

        teapotAttack.ambiantBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilSoft.asset");
        if (teapotAttack.ambiantBoilingWaterSfx != null)
        {
            Logger.Log("BoilSoft.asset loaded");
        }
        if (teapotAttack.ambiantBoilingWaterSfx == null)
        {
            Logger.LogError("BoilSoft.asset not found in bundle");
        }

        Logger.Log("Loading TeapotSpillHit prefab from bundle");
        GameObject teapotDropletSpillHit = bundle.LoadAsset<GameObject>("TeapotSpillHit.prefab");
        if (teapotDropletSpillHit != null)
        {
            Logger.Log("TeapotSpillHit prefab loaded, configuring particle system material");
            var psRenderer = teapotDropletSpillHit.GetComponent<ParticleSystem>()?.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.material = GameMaterials.GetMaterial(GameMaterial.M_Pool_7);
                Logger.Log("Applied M_ShopGlass material to TeapotSpillHit particle system");
            }
            if (psRenderer == null)
            {
                Logger.LogError("ParticleSystemRenderer not found on TeapotSpillHit");
            }
        }
        if (teapotDropletSpillHit == null)
        {
            Logger.LogError("TeapotSpillHit.prefab not found in bundle");
        }

        TeapotFinal.AddComponent<TeapotContentProvider>();
        Logger.Log("TeapotContentProvider added to TeapotFinal");

        DbsContentApi.Modules.ContentEvents.RegisterEvent(new TeapotContentEvent());

        Logger.Log("Adding TeapotFinal to customMonsters list");
        DbsContentApiPlugin.customMonsters.Add(TeapotFinal);
        Logger.Log($"TeapotFinal registration completed: {TeapotFinal.name}");

        GameMaterials.OnMaterialsLoaded += () =>
        {
            GameMaterials.ApplyMaterial(TeapotFinal, GameMaterial.M_Monster, true);
            if (teapotDroplet == null)
            {
                Logger.LogError("TeapotDroplet prefab missing; skipping droplet material setup");
            }
            if (teapotDroplet != null)
            {
                var outer = teapotDroplet.transform.Find("Outer");
                var inner = teapotDroplet.transform.Find("Inner");
                if (outer == null)
                {
                    Logger.LogError("TeapotDroplet Outer transform not found");
                }
                if (outer != null)
                {
                    GameMaterials.ApplyMaterial(outer.gameObject, GameMaterial.M_ShopGlass);
                    Logger.Log("Applied M_ShopGlass material to TeapotDroplet Outer");
                }
                if (inner == null)
                {
                    Logger.LogError("TeapotDroplet Inner transform not found");
                }
                if (inner != null)
                {
                    GameMaterials.ApplyMaterial(inner.gameObject, GameMaterial.M_Pool_7);
                    Logger.Log("Applied M_Pool_7 material to TeapotDroplet Inner");
                }
            }

            if (teapotDropletSpillHit == null)
            {
                Logger.LogError("TeapotSpillHit prefab missing; skipping spill trail material");
            }
            if (teapotDropletSpillHit != null)
            {
                var teapotSpillhitParticleSystem = teapotDropletSpillHit.GetComponent<ParticleSystemRenderer>();
                if (teapotSpillhitParticleSystem == null)
                {
                    Logger.LogError("ParticleSystemRenderer not found on TeapotSpillHit for trail material");
                }
                if (teapotSpillhitParticleSystem != null)
                {
                    teapotSpillhitParticleSystem.trailMaterial = GameMaterials.GetMaterial(GameMaterial.M_Pool_7);
                }
            }
        };
    }
}

