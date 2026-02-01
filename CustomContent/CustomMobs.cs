using System.Collections.Generic;
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

    /// <summary>
    /// Configures all custom monsters using the loaded AssetBundle.
    /// </summary>
    /// <param name="bundle">The AssetBundle containing monster prefabs.</param>
    public static void Setup(AssetBundle bundle)
    {
        Logger.Log("Starting custom mobs setup");

        // Logger.Log("Loading MainCharacter prefab from bundle");
        // MainCharacter = ContentLoader.LoadPrefabFromBundle(bundle, "MainCharacter.prefab");
        // if (MainCharacter == null)
        // {
        //     Logger.LogError("Failed to load MainCharacter.prefab from bundle");
        // }
        // else
        // {
        //     Logger.Log($"MainCharacter prefab loaded: {MainCharacter.name}");
        // }

        Logger.Log("Loading TeapotFinal prefab from bundle");
        TeapotFinal = ContentLoader.LoadPrefabFromBundle(bundle, "TeapotFinal.prefab");
        if (TeapotFinal == null)
        {
            Logger.LogError("Failed to load TeapotFinal.prefab from bundle");
        }
        else
        {
            Logger.Log($"TeapotFinal prefab loaded: {TeapotFinal.name}");
        }

        // RegisterMainCharacter();
        RegisterTeapot(bundle);

        Logger.Log("Custom mobs setup completed");
    }

    private static void RegisterMainCharacter()
    {
        if (MainCharacter == null)
        {
            Logger.LogWarning("MainCharacter is null, skipping registration");
            return;
        }

        Logger.Log($"Registering MainCharacter: {MainCharacter.name}");

        Logger.Log("Restoring shaders for MainCharacter");
        Mobs.RestoreShaders(MainCharacter);

        Logger.Log("Creating bodyparts configuration for MainCharacter");
        var mainCharacterBodyparts = new List<RigCreatorBodypart> {
            Mobs.CreatePart(BodypartType.Hip, 50f, ColliderType.Box, hasJoint: false),
            Mobs.CreatePart(BodypartType.Spine_1, 25f, ColliderType.Box),
            Mobs.CreatePart(BodypartType.Head, 10f, ColliderType.Sphere),
            Mobs.CreatePart(BodypartType.Arm_L, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Elbow_L, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Arm_R, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Elbow_R, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Leg_L, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Knee_L, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Foot_L, 5f, ColliderType.Box),
            Mobs.CreatePart(BodypartType.Leg_R, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Knee_R, 5f, ColliderType.Capsule),
            Mobs.CreatePart(BodypartType.Foot_R, 5f, ColliderType.Box),
        };

        var config = new MobSetupConfig
        {
            visualRig = new RigCreatorConfig
            {
                bodyparts = mainCharacterBodyparts
            },
            budget = new BudgetConfig(),
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            monsterAnimationValues = new MonsterAnimationValuesConfig { rightPunch = false, leftPunch = false, movementMultiplier = 1.5f },
            ragdoll = new RagdollConfig(),
            addMonsterSyncer = true,
            addAnimRefHandler = true,
            photonView = new PhotonViewConfig(),
            bot = new BotConfig { monsterName = MainCharacter.name },
            navMesh = new NavMeshConfig(),
            addMonsterAnimationHandler = true,
            addHeadFollower = true,
            addGroundPos = true
        };
        Logger.Log($"Created MobSetupConfig with {mainCharacterBodyparts.Count} bodyparts");

        Logger.Log("Setting up MainCharacter custom monster");
        Mobs.SetupCustomMonster(MainCharacter, MainCharacter.name, config);

        Logger.Log("Adding Bot_Chaser component to MainCharacter");
        Mobs.AddBotChaserComponent(Mobs.GetBotChildObject(MainCharacter));

        Logger.Log("Adding MainCharacter to customMonsters list");
        DbsContentApiPlugin.customMonsters.Add(MainCharacter);
        Logger.Log($"MainCharacter registration completed: {MainCharacter.name}");
    }

    private static void RegisterTeapot(AssetBundle bundle)
    {
        if (TeapotFinal == null)
        {
            Logger.LogError("TeapotFinal is null, skipping registration");
            return;
        }

        Logger.Log($"Registering TeapotFinal: {TeapotFinal.name}");

        Logger.Log("Restoring shaders for TeapotFinal");
        Mobs.RestoreShaders(TeapotFinal);

        Logger.Log("Creating MobSetupConfig for TeapotFinal (using existing RigCreator setup)");
        var config = new MobSetupConfig
        {
            // visualRig = null means use the existing RigCreator + PlayerVisual configuration on the prefab
            visualRig = null,
            budget = new BudgetConfig { budgetCost = 2, rarity = 1f },
            controller = new ControllerConfig(),
            player = new PlayerConfig(),
            ragdoll = new RagdollConfig(),
            photonView = new PhotonViewConfig(),
            bot = new BotConfig { monsterName = TeapotFinal.name },
            navMesh = new NavMeshConfig(),
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
        var beakTransform = TeapotFinal?.transform.Find("Visual/TeapotFinal/Armature/Hip/Spine_1/Head/Beak");
        teapotAttack.beakTransform = beakTransform!;
        if (beakTransform != null)
        {
            Logger.Log("Beak transform found and assigned to Attack_Teapot");
        }
        else
        {
            Logger.LogWarning("Beak transform not found for Attack_Teapot");
        }

        Logger.Log("Loading TeapotDroplet prefab from bundle");
        GameObject teapotDroplet = bundle.LoadAsset<GameObject>("TeapotDroplet.prefab");
        if (teapotDroplet != null)
        {
            Logger.Log("TeapotDroplet prefab loaded, configuring materials");
            teapotAttack.waterProjectilePrefab = teapotDroplet;
            var outer = teapotDroplet.transform.Find("Outer");
            var inner = teapotDroplet.transform.Find("Inner");
            if (outer != null)
            {
                GameMaterials.ApplyMaterial(outer.gameObject, GameMaterialType.M_ShopGlass);
                Logger.Log("Applied M_ShopGlass material to TeapotDroplet Outer");
            }
            if (inner != null)
            {
                GameMaterials.ApplyMaterial(inner.gameObject, GameMaterialType.M_Pool_7);
                Logger.Log("Applied M_Pool_7 material to TeapotDroplet Inner");
            }
        }
        else
        {
            Logger.LogWarning("TeapotDroplet.prefab not found in bundle");
        }

        Logger.Log("Loading SFX assets for TeapotFinal");
        teapotAttack.hardBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilHard.asset");
        if (teapotAttack.hardBoilingWaterSfx != null)
        {
            Logger.Log("BoilHard.asset loaded");
        }
        else
        {
            Logger.LogWarning("BoilHard.asset not found in bundle");
        }

        teapotAttack.ambiantBoilingWaterSfx = bundle.LoadAsset<SFX_Instance>("BoilSoft.asset");
        if (teapotAttack.ambiantBoilingWaterSfx != null)
        {
            Logger.Log("BoilSoft.asset loaded");
        }
        else
        {
            Logger.LogWarning("BoilSoft.asset not found in bundle");
        }

        Logger.Log("Loading TeapotSpillHit prefab from bundle");
        GameObject teapotDropletSpill = bundle.LoadAsset<GameObject>("TeapotSpillHit.prefab");
        if (teapotDropletSpill != null)
        {
            Logger.Log("TeapotSpillHit prefab loaded, configuring particle system material");
            var psRenderer = teapotDropletSpill.GetComponent<ParticleSystem>()?.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                psRenderer.material = GameMaterials.GetMaterial(GameMaterialType.M_ShopGlass);
                Logger.Log("Applied M_ShopGlass material to TeapotSpillHit particle system");
            }
            else
            {
                Logger.LogWarning("ParticleSystemRenderer not found on TeapotSpillHit");
            }
        }
        else
        {
            Logger.LogWarning("TeapotSpillHit.prefab not found in bundle");
        }

        Logger.Log("Adding TeapotFinal to customMonsters list");
        if (TeapotFinal != null)
        {
            DbsContentApiPlugin.customMonsters.Add(TeapotFinal);
            Logger.Log($"TeapotFinal registration completed: {TeapotFinal.name}");
        }

        if(TeapotFinal != null)
        {
            TeapotFinal.AddComponent<TeapotContentProvider>();
            Logger.Log("TeapotContentProvider added to TeapotFinal");
        }

        DbsContentApi.Modules.ContentEvents.RegisterEvent(new TeapotContentEvent());
    }
}

