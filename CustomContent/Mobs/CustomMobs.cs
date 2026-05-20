using UnityEngine;
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

        TeapotFinal = ContentLoader.LoadPrefabFromBundle(bundle, "TeapotFinal.prefab");
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

        var config = new MobSetupConfig
        {
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 1, rarity = 1f },
            controller = new ControllerConfig { gravity = 20f, movementForce = 20f },
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig(),
            navMesh = new NavMeshAgentConfig { height = 1.8f, radius = 0.64f, speed = 3.5f, wide = true },
            monsterAnimationValues = new MonsterAnimationValuesConfig { rightPunch = false, leftPunch = false },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        Mobs.RegisterMonster(ReaperPrefab, "Reaper", config,
            material: GameMaterial.M_Monster,
            postSetup: go =>
            {
                GameObject botReaper = Mobs.GetBotChildObject(go);
                Mobs.AddBotChaserComponent(botReaper);
                go.AddComponent<ReaperContentProvider>();
                DbsContentApi.ContentEvents.RegisterEvent(new ReaperContentEvent());
            });
    }

    private static void RegisterTeapot(AssetBundle bundle)
    {
        if (TeapotFinal == null)
        {
            Logger.LogError("TeapotFinal prefab failed to load; skipping Teapot registration");
            return;
        }

        var config = new MobSetupConfig
        {
            visualRig = null,
            budget = new BudgetConfig { budgetCost = UnlistedEntities.DEBUG_MODE ? 1 : 3, rarity = 0.5f },
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig(),
            navMesh = new NavMeshAgentConfig { height = 2f, radius = 1f, speed = 3.5f, wide = true },
            monsterAnimationValues = new MonsterAnimationValuesConfig { rightPunch = false, leftPunch = false },
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true,
        };

        GameObject? teapotDroplet = null;
        GameObject? teapotDropletSpillHit = null;

        Mobs.RegisterMonster(TeapotFinal, "TeapotFinal", config,
            material: GameMaterial.M_Monster,
            postSetup: go =>
            {
                GameObject botTeapot = Mobs.GetBotChildObject(go);
                Mobs.AddBotChaserComponent(botTeapot);

                var teapotAttack = botTeapot.AddComponent<Attack_Teapot>();
                var beakTransform = go.transform.Find("Visual/TeapotFinal/Armature/Hip/Spine_1/Head/Beak");
                teapotAttack.beakTransform = beakTransform;

                teapotDroplet = bundle.LoadAsset<GameObject>("TeapotDroplet.prefab");
                if (teapotDroplet != null)
                {
                    teapotAttack.waterProjectilePrefab = teapotDroplet;
                }

                teapotAttack.hardBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilHard.asset");
                teapotAttack.ambiantBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilSoft.asset");

                teapotDropletSpillHit = bundle.LoadAsset<GameObject>("TeapotSpillHit.prefab");
                if (teapotDropletSpillHit != null)
                {
                    var spillPs = teapotDropletSpillHit.GetComponent<ParticleSystem>();
                    if (spillPs != null)
                        GameMaterials.ApplyMaterial(spillPs, GameMaterial.M_Pool_7);
                }

                go.AddComponent<TeapotContentProvider>();
                DbsContentApi.ContentEvents.RegisterEvent(new TeapotContentEvent());
            });

        GameMaterials.OnMaterialsLoaded += () =>
        {
            if (teapotDroplet != null)
            {
                GameMaterials.ApplyMaterial(teapotDroplet, "Outer", GameMaterial.M_ShopGlass);
                GameMaterials.ApplyMaterial(teapotDroplet, "Inner", GameMaterial.M_Pool_7);
            }

            if (teapotDropletSpillHit != null)
            {
                var spillPs = teapotDropletSpillHit.GetComponent<ParticleSystem>();
                if (spillPs != null)
                    GameMaterials.ApplyMaterial(spillPs, GameMaterial.M_Pool_7, trailMaterial: true);
            }
        };
    }
}
