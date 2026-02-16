using System;
using Photon.Pun;
using UnityEngine;
using DbsContentApi.Modules;

/// <summary>
/// Grenade with persisted state: primed (pin pulled) and fuse countdown.
/// Uses OnOffEntry = primed, LifeTimeEntry = fuse left, StashAbleEntry = can't stash when primed.
/// Ticking AudioLoop behaviour copied from ItemGooBall (stolen from item database).
/// </summary>
public class GrenadeItemBehaviour : ItemInstanceBehaviour
{
	protected Player? player;
	protected GameObject? explosionPrefab;
	protected OnOffEntry? primedEntry;
	protected LifeTimeEntry? fuseEntry;
	protected StashAbleEntry? stashAbleEntry;
	protected AudioLoop? tickingSoundAudioLoop;

	[Header("Base Grenade Settings")]
	[SerializeField] protected float fuseTime = 5f;
	[SerializeField] protected float timeNotHeldBeforeImpactExplode = 0.1f;

	[Header("Audio Settings")]
	public bool hasTickingSound = false;
	public AudioClip? tickingSoundClip;
	public float baseTickingPitch = 0.3f;
	public float tickingPitchMultiplier = 0.9f;
	public float maxTickingPitch = 5f;
	public float minTickingPitch = 1f;
	public float tickingVolume = 0.2f;

	protected float timeNotHeld;
	protected bool exploded;

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();

		// Primed state (pin pulled) - persisted
		if (!data.TryGetEntry<OnOffEntry>(out primedEntry))
		{
			primedEntry = new OnOffEntry { on = false };
			data.AddDataEntry(primedEntry);
		}

		// Fuse countdown when primed - persisted so fuse continues after throw / on load
		if (!data.TryGetEntry<LifeTimeEntry>(out fuseEntry))
		{
			fuseEntry = new LifeTimeEntry { m_lifeTimeLeft = fuseTime, m_maxLifeTime = fuseTime };
			data.AddDataEntry(fuseEntry);
		}

		// Can't stash once primed (like ItemGooBall)
		if (!data.TryGetEntry<StashAbleEntry>(out stashAbleEntry))
		{
			stashAbleEntry = new StashAbleEntry { isStashAble = true };
			data.AddDataEntry(stashAbleEntry);
		}

		// Setup ticking sound if enabled
		if (hasTickingSound)
			EnsureTickingSound();

		// Find explosion prefab
		Item? bombItem = Items.GetItemByPrefabComponent<BombItem>();
		if (bombItem?.itemObject != null)
		{
			BombItem? bombItemBehaviour = bombItem.itemObject.GetComponent<BombItem>();
			explosionPrefab = bombItemBehaviour.explosion;
		}
		else
		{
			DbsContentApi.Modules.Logger.LogError("GrenadeItemBehaviour: Could not find BombItem prefab.");
		}
	}

	protected virtual void EnsureTickingSound()
	{
		if (tickingSoundAudioLoop != null) return;

		Item? gooItem = Items.GetItemByPrefabComponent<ItemGooBall>();
		if (gooItem?.itemObject == null) return;

		ItemGooBall? gooBall = gooItem.itemObject.GetComponent<ItemGooBall>();
		if (gooBall?.tickingSound == null) return;

		AudioLoop template = gooBall.tickingSound;
		tickingSoundAudioLoop = gameObject.GetComponent<AudioLoop>();
		if (tickingSoundAudioLoop == null)
			tickingSoundAudioLoop = gameObject.AddComponent<AudioLoop>();
		tickingSoundAudioLoop.enabled = false;
		tickingSoundAudioLoop.clip = tickingSoundClip != null ? tickingSoundClip : template.clip;
		tickingSoundAudioLoop.mixerGroup = template.mixerGroup;
		tickingSoundAudioLoop.volume = template.volume;
		tickingSoundAudioLoop.pitch = template.pitch;
		tickingSoundAudioLoop.minDistance = template.minDistance;
		tickingSoundAudioLoop.maxDistance = template.maxDistance;
		tickingSoundAudioLoop.obstrability = template.obstrability;
		tickingSoundAudioLoop.blend = template.blend;
		tickingSoundAudioLoop.volume = tickingVolume;
	}

	protected virtual void Update()
	{
		if (primedEntry == null || fuseEntry == null || stashAbleEntry == null)
			return;

		bool primed = primedEntry.on && !exploded;

		UpdateTickingSound(primed);

		// Track time not held (avoid impact-detonate the frame we throw)
		if (!isHeld) timeNotHeld += Time.deltaTime;
		else timeNotHeld = 0f;

		// Handle input
		HandleInput();

		// Fuse countdown when primed
		if (primed)
			fuseEntry.m_lifeTimeLeft -= Time.deltaTime;

		// Explode by time when in world (not held)
		if (primed && fuseEntry.m_lifeTimeLeft <= 0f && !isHeld)
			SpawnExplosion();
	}

	protected virtual void UpdateTickingSound(bool primed)
	{
		if (tickingSoundAudioLoop != null && hasTickingSound)
		{
			if (primed != tickingSoundAudioLoop.enabled)
				tickingSoundAudioLoop.enabled = primed;

			if (primed && fuseEntry != null)
			{
				float calculatedPitch = (fuseTime * tickingPitchMultiplier) / Mathf.Max(0.01f, fuseEntry.m_lifeTimeLeft) + baseTickingPitch;
				tickingSoundAudioLoop.pitch = Mathf.Clamp(calculatedPitch, minTickingPitch, maxTickingPitch);
			}
		}
	}

	protected virtual void HandleInput()
	{
		if (isHeldByMe && !Player.localPlayer.HasLockedInput() && Player.localPlayer.input.clickWasPressed && Player.localPlayer.TryGetInventory(out var o) && o.TryGetSlot(Player.localPlayer.data.selectedItemSlot, out var slot))
		{
			OnPrimaryClick(slot.SlotID);
		}
	}

	protected virtual void OnPrimaryClick(int slotID)
	{
		DbsContentApi.Modules.Logger.Log($"GrenadeItemBehaviour: Grenade thrown by {Player.localPlayer.gameObject.name}.");
		Action callbackOnThrowFrame = () =>
		{
			if (player != null && player.refs != null && player.refs.items != null && player.data != null && primedEntry != null && stashAbleEntry != null && fuseEntry != null)
			{
				primedEntry.on = true;
				stashAbleEntry.isStashAble = false;
				fuseEntry.m_lifeTimeLeft = fuseTime;
				fuseEntry.m_maxLifeTime = fuseTime;
				stashAbleEntry.SetDirty();
				primedEntry.SetDirty();
				DbsContentApi.Modules.Logger.Log("GrenadeItemBehaviour: Grenade primed.");
				player.data.throwCharge = 1f;
				player.refs.items.DropItem(slotID);
			}
		};
		var animRig = player!.gameObject.transform.Find("AnimationRig");
		if (animRig != null && animRig.TryGetComponent<CustomPlayerAnimator>(out var animator))
			animator.TryActivateThrowAnimation(callbackOnThrowFrame);
	}

	protected virtual void SpawnExplosion()
	{
		if (exploded) return;
		exploded = true;

		OnExplode();

		PhotonView pv = GetComponentInParent<PhotonView>();
		if (pv != null)
			PhotonNetwork.Destroy(pv.gameObject);
	}

	protected virtual void OnExplode()
	{
		if (explosionPrefab != null)
			UnityEngine.Object.Instantiate(explosionPrefab, base.transform.position, base.transform.rotation);
	}
}