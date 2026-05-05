using Photon.Pun;
using UnlistedEntities.CustomContent.ContentEvents;
using DbsContentApi.Modules.Utility;
using UnityEngine;

public class TemporaryInvisibilityItemBehaviour : ItemInstanceBehaviour
{
	private Player? player;
	public SFX_Instance invisibilitySpraySfx;
	public static float duration = 8f;

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();
	}
	private void Update()
	{
		if (isHeldByMe && !Player.localPlayer.HasLockedInput() && Player.localPlayer.input.clickWasPressed && Player.localPlayer.TryGetInventory(out var o) && o.TryGetSlot(Player.localPlayer.data.selectedItemSlot, out var slot))
		{
			PlayerRPCBridge bridge = Player.localPlayer.gameObject.GetComponent<PlayerRPCBridge>();
			if (bridge == null)
			{
				DbsContentApi.Modules.Logger.LogError($"TemporaryInvisibilityItemBehaviour: Could not find PlayerRPCBridge on {Player.localPlayer.gameObject.name}.");
				return;
			}
			if (bridge.isInvisibilityActive)
			{
				DbsContentApi.Modules.Logger.LogError($"TemporaryInvisibilityItemBehaviour: Invisibility is already active on {Player.localPlayer.gameObject.name}.");
			}
			bridge!.view.RPC(nameof(PlayerRPCBridge.RPCA_Make_Invisible), RpcTarget.All, bridge.view.ViewID, duration);
			Player.localPlayer.refs.emotes.DoBookEquipEffect(Player.localPlayer.refs.view.ViewID, itemInstance.item.id, base.transform.position, base.transform.rotation);

			// Spawn a temporary content trigger attached to the player for the duration of invisibility
			Transform hip = Player.localPlayer.transform.Find("RigCreator/Rig/Armature/Hip");
			if (hip != null)
			{
				int frameLifetime = Mathf.RoundToInt(60f * duration);
				GameObject trigger = ObjectHelper.CreateAttachedTriggerObject(UnlistedEntities.CustomContent.CustomItems.TemporaryContentTriggerPrefab!, hip, frameLifetime);

				var provider = trigger.AddComponent<InvisiblePlayerContentProvider>();
				provider.playerName = Player.localPlayer.refs.view.Owner.NickName;
				provider.actorNumber = Player.localPlayer.refs.view.Owner.ActorNumber;
			}

			if (player == Player.localPlayer)
			{
				invisibilitySpraySfx.Play(base.transform.position);
			}
			slot.Clear();
		}
	}
}