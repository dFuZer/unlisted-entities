using HarmonyLib;
using UnityEngine;
using UnlistedEntities.CustomContent;

/// <summary>
/// Equipable item that grants increased jump power when worn.
/// Inherits from EquipableItemBehaviour for standard equipable logic.
/// </summary>
public class AngelWingsEquipableItemBehaviour : EquipableItemBehaviour
{
	// All equipable logic is handled by the base class
	// Override virtual methods here if custom behavior is needed
}

[HarmonyPatch(typeof(Player))]
public class AngelWingsEquipableItemBehaviourPatches
{
	[HarmonyPatch("CallTakeDamageAndAddForceAndFall"), HarmonyPrefix]
	public static bool PrefixCallTakeDamageAndAddForceAndFall(Player __instance, float damage, Vector3 force, float fall)
	{
		// using the call values we verify if this is an impact fall. we then check if the player has the angel wings equipable item.
		// if both are true, we cancel the fall.
		if (damage == 0f && force == Vector3.zero && fall == 2f && __instance.IsLocal && EquipableInventory.PlayerHasEquipableCached(__instance, CustomItems.AngelWingsItem!.id))
		{
			return false;
		}
		return true;
	}
}

[HarmonyPatch(typeof(PlayerController), "FixedUpdate")]
public static class AngelWingsVelocityPatch
{

	[HarmonyPostfix]
	public static void Postfix(PlayerController __instance)
	{
		// DbsContentApi.Modules.Logger.Log($"FixedUpdate called for {__instance.name}");
		Player player = __instance.GetComponent<Player>();
		if (player == null) return;

		var hasAngelWings = EquipableInventory.PlayerHasEquipableCached(player, CustomItems.AngelWingsItem!.id);
		if (!hasAngelWings) return;

		// If the player is grounded, we don't need to cap velocity
		if (player.data.isGrounded) return;

		// Cap downwards velocity for all body parts
		PlayerRagdoll ragdoll = player.refs.ragdoll;
		if (ragdoll == null) return;

		var hip = ragdoll.GetBodypart(BodypartType.Torso);
		if (hip == null) return;
		{
			Vector3 velocity = hip.rig.velocity;
			if (velocity.y < -AngelWingsVisualAnimationHandler.MAX_DOWNWARDS_VELOCITY)
			{
				velocity.y = -AngelWingsVisualAnimationHandler.MAX_DOWNWARDS_VELOCITY;
				hip.rig.velocity = velocity;
			}
		}

		// foreach (var rig in ragdoll.rigList)
		// {
		// 	Vector3 velocity = rig.velocity;
		// 	DbsContentApi.Modules.Logger.Log($"Velocity for {rig.name} is {velocity}");
		// 	// gravityDirection is usually Vector3.down, so we check velocity in that direction
		// 	// If velocity.y is negative (downwards), we cap it.
		// 	if (velocity.y < -MAX_DOWNWARDS_VELOCITY)
		// 	{
		// 		DbsContentApi.Modules.Logger.Log($"Capping downwards velocity for {rig.name} to {MAX_DOWNWARDS_VELOCITY}");
		// 		velocity.y = -MAX_DOWNWARDS_VELOCITY;
		// 		rig.velocity = velocity;
		// 	}
		// }
	}
}

public class AngelWingsVisualAnimationHandler : MonoBehaviour
{
	private Animator animator;
	private Player player;
	private Bodypart hip;
	private bool flying = false;
	public static float MAX_DOWNWARDS_VELOCITY = 1.7f;
	private void Start()
	{
		animator = GetComponent<Animator>();
		player = GetComponentInParent<Player>();
		hip = player.refs.ragdoll.GetBodypart(BodypartType.Torso);
	}

	private void LateUpdate()
	{
		if (player.data.isGrounded)
		{
			if (flying)
			{
				SetFlying(false);
			}
			return;
		}

		if (gameObject.activeSelf && hip != null)
		{
			Vector3 velocity = hip.rig.velocity;
			// set to true when falling faster than -1.6f
			if (velocity.y < -MAX_DOWNWARDS_VELOCITY * 0.80f && !flying && !player.data.isGrounded)
			{
				SetFlying(true);
			}
			// set to false when falling slower than -1.4f 
			if (velocity.y > -MAX_DOWNWARDS_VELOCITY * 0.65f && flying)
			{
				SetFlying(false);
			}
		}
	}

	public void SetFlying(bool flying)
	{
		DbsContentApi.Modules.Logger.Log($"-------------- Setting flying to {flying}");
		this.flying = flying;
		animator.SetBool("Flying", value: flying);
	}
}