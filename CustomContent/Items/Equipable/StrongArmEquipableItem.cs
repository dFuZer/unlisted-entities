using Photon.Pun;
using HarmonyLib;
using UnityEngine;
using UnlistedEntities.CustomContent;

/// <summary>
/// Equipable item that grants increased jump power when worn.
/// Inherits from EquipableItemBehaviour for standard equipable logic.
/// </summary>
public class StrongArmEquipableItemBehaviour : EquipableItemBehaviour
{
	// All equipable logic is handled by the base class
	// Override virtual methods here if custom behavior is needed
}

// patch VideoCamera:ConfigItem to add a new component to it when its held by a player with the strong arm equipable item equipped.
[HarmonyPatch(typeof(VideoCamera))]
public static class VideoCameraConfigItemPatch
{
	[HarmonyPatch("ConfigItem"), HarmonyPostfix]
	public static void ConfigItemPostfix(VideoCamera __instance, ItemInstanceData data, PhotonView playerView)
	{
		var player = __instance.GetComponentInParent<Player>();
		if (player == null) return;
		var hasStrongArm = EquipableInventory.PlayerHasEquipableCached(player, CustomItems.StrongArmItem!.id);

		if (hasStrongArm)
		{
			__instance.gameObject.AddComponent<StrongArmVideoCameraComponent>();
		}
	}
}

public class StrongArmVideoCameraComponent : MonoBehaviour
{
	private Player player = null!;
	private Rigidbody rb = null!;
	void Start()
	{
		player = GetComponentInParent<Player>();
		rb = GetComponent<Rigidbody>();
	}
	void LateUpdate()
	{
		if (player == null || rb == null) return;

		// The camera is attached to the player ragdoll's hand/arm.
		// We want to stabilize the camera by smoothing its movement.
		// We do this by calculating a target rotation based on the player's look direction
		// and applying it to the camera's rigidbody.

		var head = player.refs.ragdoll.GetBodypart(BodypartType.Head);
		if (head == null) return;

		// Target rotation is the player's head rotation (where they are looking)
		Quaternion targetRotation = head.transform.rotation;

		// Smoothly interpolate the rotation
		rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);
		rb.angularVelocity = Vector3.zero;
	}
}