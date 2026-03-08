using System;
using Photon.Pun;
using UnityEngine;
using DbsContentApi.Modules;

/// <summary>
/// Electric Grenade with specific visual sync logic.
/// </summary>
public class ElectricGrenadeItemBehaviour : ThrowableExplosiveBehaviour
{
	// Animator driving the grenade's visual state (primed/unprimed)
	private Animator? grenadeAnimator;
	private bool lastPrimedVisualState;

	private string primedStateName = "electric_grenade_prime";

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		base.ConfigItem(data, playerView);

		// Cache animator reference (works both in hand and when dropped)
		CacheAnimator();

		// Snap to persisted primed state without playing transition (restore on spawn/pickup/drop)
		SyncPrimedVisuals(playTransition: false);
	}

	private void CacheAnimator()
	{
		var grenadeTransform = gameObject.transform.Find("Item/electric-grenade");
		if (grenadeTransform != null)
			grenadeAnimator = grenadeTransform.GetComponent<Animator>();
	}

	protected override void Update()
	{
		if (primedEntry == null || fuseEntry == null || stashAbleEntry == null)
			return;

		bool primed = primedEntry.on && !exploded;

		// Ensure animator reference (in case of runtime re-parenting / pooling)
		if (grenadeAnimator == null)
			CacheAnimator();

		// Keep grenade visuals in sync with persisted primed state (snap, never replay transition)
		if (grenadeAnimator != null && primed != lastPrimedVisualState)
			SyncPrimedVisuals(playTransition: false);

		base.Update();
	}

	protected override void OnPrimaryClick(int slotID)
	{
		if (primedEntry != null && !primedEntry.on)
		{
			primedEntry.on = true;
			if (stashAbleEntry != null) stashAbleEntry.isStashAble = false;
			if (fuseEntry != null)
			{
				fuseEntry.m_lifeTimeLeft = fuseTime;
				fuseEntry.m_maxLifeTime = fuseTime;
			}
			primedEntry.SetDirty();
			if (stashAbleEntry != null) stashAbleEntry.SetDirty();
			SyncPrimedVisuals(playTransition: true);
		}

		base.OnPrimaryClick(slotID);
	}

	/// <summary>
	/// Syncs the grenade visuals with the persisted primed state.
	/// </summary>
	/// <param name="playTransition">If true, plays the Primed transition (use only when user first primes in hand).
	/// If false, snaps directly to the target state without animation (use for throw, pickup, drop, load).</param>
	private void SyncPrimedVisuals(bool playTransition = false)
	{
		if (grenadeAnimator == null)
			CacheAnimator();

		if (grenadeAnimator == null || primedEntry == null)
			return;

		bool primed = primedEntry.on && !exploded;
		if (primed == lastPrimedVisualState)
			return;

		if (primed)
		{
			if (playTransition)
				grenadeAnimator.SetTrigger("Primed");
			else
				grenadeAnimator.Play(primedStateName, 0, 1f); // Snap to end of primed state
		}
		else
		{
			grenadeAnimator.SetBool("Primed", false);
		}
		lastPrimedVisualState = primed;
	}
}