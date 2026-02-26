using HarmonyLib;
using UnityEngine;
using System.Collections;
using UnlistedEntities.CustomContent;
using Photon.Voice.Unity.UtilityScripts;

/// <summary>
/// Equipable item that grants increased jump power when worn.
/// Inherits from EquipableItemBehaviour for standard equipable logic.
/// </summary>
public class BootsEquipableItemBehaviour : EquipableItemBehaviour
{
	// All equipable logic is handled by the base class
	// Override virtual methods here if custom behavior is needed
}

/// <summary>
/// Harmony patch that boosts player jump power when Jumping Boots are equipped.
/// </summary>
[HarmonyPatch(typeof(PlayerController))]
public class JumpPatch
{
	[HarmonyPatch("RPCA_Jump"), HarmonyPrefix]
	static void PrefixPatchJumpImpulse(PlayerController __instance)
	{
		var player = __instance.GetComponent<Player>();
		var hasBootsEquipable = EquipableInventory.PlayerHasEquipableCached(player, CustomItems.JumpingBootsItem!.id); ;

		// Check if player has jumping boots equipped using the actual item ID
		if (hasBootsEquipable)
		{
			__instance.StartCoroutine(BoostJumpTemporarily(__instance));
		}
	}

	private static IEnumerator BoostJumpTemporarily(PlayerController controller)
	{
		float originalJumpImpulse = controller.jumpImpulse;
		float originalJumpForceOverTime = controller.jumpForceOverTime;
		float originalJumpForceDuration = controller.jumpForceDuration;

		controller.jumpImpulse = 15f;
		controller.jumpForceOverTime = 0.7f;
		controller.jumpForceDuration = 1f;

		yield return new WaitForSeconds(1f);

		controller.jumpImpulse = originalJumpImpulse;
		controller.jumpForceOverTime = originalJumpForceOverTime;
		controller.jumpForceDuration = originalJumpForceDuration;
	}
}
