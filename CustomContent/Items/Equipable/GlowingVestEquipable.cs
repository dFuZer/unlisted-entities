using Photon.Pun;
using HarmonyLib;
using UnityEngine;
using UnlistedEntities.CustomContent;
using UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Equipable item that grants increased jump power when worn.
/// Inherits from EquipableItemBehaviour for standard equipable logic.
/// </summary>
public class GlowingVestEquipableItemBehaviour : EquipableItemBehaviour
{
	protected override void OnEquipped(int slotIndex)
	{
		base.OnEquipped(slotIndex);
		var player = GetComponentInParent<Player>();
		if (player != null && player.refs.view.IsMine)
		{
			var provider = player.gameObject.AddComponent<GlowingVestContentProvider>();
			provider.playerName = player.refs.view.Owner.NickName;
			provider.actorNumber = player.refs.view.Owner.ActorNumber;
		}
	}

	protected override void OnUnequipped(int slotIndex)
	{
		base.OnUnequipped(slotIndex);
		var player = GetComponentInParent<Player>();
		if (player != null)
		{
			var provider = player.gameObject.GetComponent<GlowingVestContentProvider>();
			if (provider != null) Destroy(provider);
		}
	}
}