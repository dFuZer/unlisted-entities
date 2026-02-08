using Photon.Pun;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using System.Collections;
public class BootsEquipableItemBehaviour : ItemInstanceBehaviour
{
	private Player? player;

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();
	}
	private void Update()
	{
		if (isHeldByMe && !Player.localPlayer.HasLockedInput() && Player.localPlayer.input.clickWasPressed && Player.localPlayer.TryGetInventory(out var o) && o.TryGetSlot(Player.localPlayer.data.selectedItemSlot, out var slot))
		{
			EquipableInventory inventory = o.gameObject.GetComponent<EquipableInventory>();
			if (inventory == null)
			{
				DbsContentApi.Modules.Logger.LogError($"BootsEquipableItemBehaviour: Could not find EquipableInventory on {Player.localPlayer.gameObject.name}.");
				return;
			}
			inventory.SetEquipable(0, 0);
			Player.localPlayer.refs.emotes.DoBookEquipEffect(Player.localPlayer.refs.view.ViewID, itemInstance.item.id, base.transform.position, base.transform.rotation);
			slot.Clear();
		}
	}
}

[HarmonyPatch(typeof(PlayerController))]
public class JumpPatch
{
	[HarmonyPatch("RPCA_Jump"), HarmonyPrefix]
	static void PrefixPatchJumpImpulse(PlayerController __instance)
	{
		DbsContentApi.Modules.Logger.Log("PrefixPatchJumpImpulse");
		var player = __instance.GetComponent<Player>();
		player.TryGetInventory(out var inventory);
		if (inventory)
		{
			DbsContentApi.Modules.Logger.Log("PrefixPatchJumpImpulse: Inventory found");
			var equipables = inventory.gameObject.GetComponent<EquipableInventory>();
			var hasJumpingBoots = equipables.equipableIDs.Any(e => e == 0);
			if (hasJumpingBoots)
			{
				DbsContentApi.Modules.Logger.Log("PrefixPatchJumpImpulse: Has jumping boots, running temporary boost");
				var playerController = player.GetComponent<PlayerController>();
				__instance.StartCoroutine(BoostJumpTemporarily(playerController));
			}
			else
			{
				DbsContentApi.Modules.Logger.Log("PrefixPatchJumpImpulse: No jumping boots");
			}
		}
		else
		{
			DbsContentApi.Modules.Logger.Log("PrefixPatchJumpImpulse: No inventory found");
		}
	}

	private static IEnumerator BoostJumpTemporarily(PlayerController controller)
	{
		float originalJumpImpulse = controller.jumpImpulse;
		float originalJumpForceOverTime = controller.jumpForceOverTime;
		float originalJumpForceDuration = controller.jumpForceDuration;
		controller.jumpImpulse = 15;
		controller.jumpForceOverTime = 0.7f;
		controller.jumpForceDuration = 1f;
		yield return new WaitForSeconds(1f);
		controller.jumpImpulse = originalJumpImpulse;
		controller.jumpForceOverTime = originalJumpForceOverTime;
		controller.jumpForceDuration = originalJumpForceDuration;
		DbsContentApi.Modules.Logger.Log("BoostJumpTemporarily: Restting to original values: " + originalJumpImpulse + " " + originalJumpForceOverTime + " " + originalJumpForceDuration);
	}
}