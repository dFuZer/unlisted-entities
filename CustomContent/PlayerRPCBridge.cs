using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using DbsContentApi.Modules;
using HarmonyLib;

public class PlayerRPCBridge : MonoBehaviour
{
    public PhotonView view = null!;

    /// <summary>True while a temporary invisibility effect is active (used to block stacking and incorrect restore).</summary>
    public bool isInvisibilityActive;

    /// <summary>True while a temporary boost effect is active (used to block stacking and incorrect restore).</summary>
    public bool isBoostActive;
    public void Start()
    {
        view = GetComponent<PhotonView>();
        if (view == null)
        {
            DbsContentApi.Modules.Logger.LogError($"PlayerRPCBridge: Could not find PhotonView on {gameObject.name}.");
        }
    }

    [PunRPC]
    public void RPCA_Make_Invisible(int playerViewId, float duration)
    {
        Player player = PlayerHandler.instance.TryGetPlayerFromViewID(playerViewId);
        if (player != null)
        {

            StartCoroutine(MakeInvisibleCoroutine(player, duration));
        }
    }

    private IEnumerator MakeInvisibleCoroutine(Player player, float duration)
    {
        isInvisibilityActive = true;
        try
        {
            if (PhotonNetwork.IsMasterClient)
            {
                List<Bot> allBots = BotHandler.instance.bots;
                foreach (Bot bot in allBots)
                {
                    if (bot.view.IsMine)
                    {
                        bot.IgnoreTargetFor(player, duration);
                    }
                    else
                    {
                        DbsContentApi.Modules.Logger.Log($"Bot {bot.name} is not mine, but I am the host. This is unexpected.");
                    }
                }
            }
            var playerObject = player.gameObject;
            GameMaterials.Materials.TryGetValue(GameMaterialType.M_ShopGlass, out Material glassMaterial);
            if (glassMaterial == null)
            {
                DbsContentApi.Modules.Logger.LogError($"PlayerRPCBridge: Could not find glass material.");
                yield break;
            }
            var bodyRenderer = playerObject.transform.Find("CharacterModel/BodyRenderer").GetComponent<Renderer>();
            var headRenderer = playerObject.transform.Find("CharacterModel/HeadRenderer").GetComponent<Renderer>();
            var originalMaterialsBodyRendererArray = bodyRenderer.materials;
            var originalMaterialsHeadRendererArray = headRenderer.materials;
            if (glassMaterial != null)
            {
                bodyRenderer.materials = new Material[] { glassMaterial };
                headRenderer.materials = new Material[] { glassMaterial };
            }

            yield return new WaitForSeconds(duration);
            bodyRenderer.materials = originalMaterialsBodyRendererArray;
            headRenderer.materials = originalMaterialsHeadRendererArray;
        }
        finally
        {
            isInvisibilityActive = false;
        }
    }

    /// <summary>
    /// Plays the custom throw animation on this player (used by other clients when this player throws).
    /// </summary>
    [PunRPC]
    public void RPCA_PlayThrowAnimation()
    {
        var animator = GetComponentInChildren<CustomPlayerAnimator>(true);
        if (animator != null)
            animator.TryActivateThrowAnimation(() => { });
    }

    [PunRPC]
    public void RPCA_Make_Boosted(int playerViewId, float duration, float moveSpeedMultiplier, float staminaInstantRegen, float staminaRegRateMultiplier)
    {
        Player player = PlayerHandler.instance.TryGetPlayerFromViewID(playerViewId);
        if (player != null)
        {
            StartCoroutine(MakeBoostedCoroutine(player, duration, moveSpeedMultiplier, staminaInstantRegen, staminaRegRateMultiplier));
        }
    }

    private IEnumerator MakeBoostedCoroutine(Player player, float duration, float moveSpeedMultiplier, float staminaInstantRegen, float staminaRegRateMultiplier)
    {
        var controller = player.gameObject.GetComponent<PlayerController>();
        if (controller == null)
            yield break;

        isBoostActive = true;
        try
        {
            var originalMovementForce = controller.movementForce;
            var originalStaminaRegRate = controller.staminaRegRate;

            controller.movementForce = originalMovementForce * moveSpeedMultiplier;
            player.data.currentStamina = Mathf.Min(player.data.currentStamina + staminaInstantRegen, controller.maxStamina);
            controller.staminaRegRate = originalStaminaRegRate * staminaRegRateMultiplier;

            yield return new WaitForSeconds(duration);

            controller.movementForce = originalMovementForce;
            controller.staminaRegRate = originalStaminaRegRate;
        }
        finally
        {
            isBoostActive = false;
        }
    }
}

[HarmonyPatch]
public class PlayerRPCBridgePatches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    [HarmonyPostfix]
    static void AddEquipableComp(GlobalPlayerData __instance)
    {
        if (__instance.gameObject.GetComponent<PlayerRPCBridge>() == null)
        {
            __instance.gameObject.AddComponent<PlayerRPCBridge>();
        }
    }
}