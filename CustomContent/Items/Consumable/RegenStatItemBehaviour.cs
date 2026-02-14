using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Zorro.Core.Serizalization;
using System.Runtime.CompilerServices;
using DbsContentApi.Modules;
using UnityEngine.UI;

public enum StatTypeEnum
{
	Oxygen = 0,
	Health = 1
}

public class RegenStatItemBehaviour : ItemInstanceBehaviour
{
	private Player? player;
	public int regenPercentageAmount;
	public StatTypeEnum statType;
	private static float maxHealth = 100;
	public SFX_Instance oxygenRegenerationSfx;
	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();
		itemInstance.RegisterRPC(ItemRPC.RPC0, RPCA_Regen);
	}

	private void RPCA_Regen(BinaryDeserializer deserializer)
	{
		int playerViewId = deserializer.ReadInt();
		StatTypeEnum readStatType = (StatTypeEnum)deserializer.ReadInt();
		float readRegenAmount = deserializer.ReadFloat();

		DbsContentApi.Modules.Logger.Log($"[RPC] Regen: {readStatType} {readRegenAmount}");

		Player player = PlayerHandler.instance.TryGetPlayerFromViewID(playerViewId);
		if (player != null)
		{
			if (readStatType == StatTypeEnum.Oxygen)
			{
				player.data.remainingOxygen = Mathf.Min(player.data.remainingOxygen + readRegenAmount, player.data.maxOxygen);
				if (player == Player.localPlayer)
				{
					var uiFeedbackExtension = UI_Feedback.instance.GetComponent<UI_FeedbackExtension>();
					if (uiFeedbackExtension != null)
					{
						uiFeedbackExtension.RegenOxygenFeedback();
						DbsContentApi.Modules.Logger.Log($"PLAYING OXYGEN REGEN SFX");
						oxygenRegenerationSfx.Play(base.transform.position);
					}
				}
			}
			else if (readStatType == StatTypeEnum.Health)
			{
				player.data.health = Mathf.Min(player.data.health + readRegenAmount, maxHealth);
				if (player == Player.localPlayer)
				{
					UI_Feedback.instance.HealFeedback();
				}
			}
		}
	}

	private void Update()
	{
		if (isHeldByMe && !Player.localPlayer.HasLockedInput() && Player.localPlayer.input.clickWasPressed && Player.localPlayer.TryGetInventory(out var o) && o.TryGetSlot(Player.localPlayer.data.selectedItemSlot, out var slot))
		{
			BinarySerializer binarySerializer = new BinarySerializer();
			binarySerializer.WriteInt(Player.localPlayer.refs.view.ViewID);
			if (statType == StatTypeEnum.Oxygen)
			{
				binarySerializer.WriteInt((int)StatTypeEnum.Oxygen);
				float gain = player.data.maxOxygen * (regenPercentageAmount / 100f);
				binarySerializer.WriteFloat(gain);
				DbsContentApi.Modules.Logger.Log($"[Update] Regen: Oxygen {gain}");
			}
			else if (statType == StatTypeEnum.Health)
			{
				binarySerializer.WriteInt((int)StatTypeEnum.Health);
				float gain = maxHealth * (regenPercentageAmount / 100f);
				binarySerializer.WriteFloat(gain);
				DbsContentApi.Modules.Logger.Log($"[Update] Regen: Health {gain}");
			}
			itemInstance.CallRPC(ItemRPC.RPC0, binarySerializer);
			Player.localPlayer.refs.emotes.DoBookEquipEffect(Player.localPlayer.refs.view.ViewID, itemInstance.item.id, base.transform.position, base.transform.rotation);
			slot.Clear();
		}
	}
}