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
			// __instance.gameObject.AddComponent<StrongArmVideoCameraComponent>();
		}
	}
}

// public class StrongArmVideoCameraComponent : MonoBehaviour
// {
// 	private Player player = null!;
// 	private Bodypart rightHand = null!;
// 	void Start()
// 	{
// 		player = GetComponentInParent<Player>();
// 		rb = GetComponent<Rigidbody>();
// 		rightHand = player.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
// 	}
// 	void LateUpdate()
// 	{
// 		if (player == null || rb == null) return;

// 		rb.velocity = Vector3.zero;
// 		rb.angularVelocity = Vector3.zero;
// 	}
// }