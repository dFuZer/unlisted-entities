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

        RegisterTeapot(bundle);

        Logger.Log("Custom mobs setup completed");
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
        // Mobs.RestoreShaders(TeapotFinal);
        GameMaterials.ApplyMaterial(TeapotFinal, GameMaterial.M_Monster, true);

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
            navMesh = new NavMeshAgentConfig { height = 1.5f, radius = 1.06f, speed = 3.5f },
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
                GameMaterials.ApplyMaterial(outer.gameObject, GameMaterial.M_ShopGlass);
                Logger.Log("Applied M_ShopGlass material to TeapotDroplet Outer");
            }
            if (inner != null)
            {
                GameMaterials.ApplyMaterial(inner.gameObject, GameMaterial.M_Pool_7);
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
                psRenderer.material = GameMaterials.GetMaterial(GameMaterial.M_ShopGlass);
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

        if (TeapotFinal != null)
        {
            TeapotFinal.AddComponent<TeapotContentProvider>();
            Logger.Log("TeapotContentProvider added to TeapotFinal");
        }

        ContentEvents.RegisterEvent(new TeapotContentEvent());

        Logger.Log("Adding TeapotFinal to customMonsters list");
        if (TeapotFinal != null)
        {
            DbsContentApiPlugin.customMonsters.Add(TeapotFinal);
            Logger.Log($"TeapotFinal registration completed: {TeapotFinal.name}");
        }
    }
}

