using Photon.Pun;
public class TemporaryPlayerBoostItemBehaviour : ItemInstanceBehaviour
{
	private Player? player;
	public static float duration = 8f;
	public static float moveSpeedMultiplier = 1.5f;
	public static float staminaInstantRegen = 2f;
	public static float staminaRegRateMultiplier = 1.2f;

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
			bridge!.view.RPC(nameof(PlayerRPCBridge.RPCA_Make_Boosted), RpcTarget.All, bridge.view.ViewID, duration, moveSpeedMultiplier, staminaInstantRegen, staminaRegRateMultiplier);
			Player.localPlayer.refs.emotes.DoBookEquipEffect(Player.localPlayer.refs.view.ViewID, itemInstance.item.id, base.transform.position, base.transform.rotation);
			slot.Clear();
		}
	}
}