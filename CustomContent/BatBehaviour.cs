using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Zorro.Core.Serizalization;

public class BatBehaviour : ItemInstanceBehaviour
{
	private Player? player;
	private bool isSwinging = false;
	private bool isInitialized = false;
	private int lastFrame;

	// Swing parameters - GENTLE to prevent self-knockdown
	public float swingForce = 3f;
	public float swingDuration = 0.6f;
	public float cooldownTime = 1.6f;

	public SFX_Instance batHitSFX = null!;

	public bool isBreakable;

	// Collision tracking
	private HashSet<Player> hitPlayers = new HashSet<Player>();

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		player = GetComponentInParent<Player>();
		itemInstance.RegisterRPC(ItemRPC.RPC0, RPC_Hit);
		itemInstance.RegisterRPC(ItemRPC.RPC1, RPC_StartSwing);
		isInitialized = true;
	}

	private void RPC_StartSwing(BinaryDeserializer deserializer)
	{
		float fdx = deserializer.ReadFloat();
		float fdy = deserializer.ReadFloat();
		float fdz = deserializer.ReadFloat();
		float ldx = deserializer.ReadFloat();
		float ldy = deserializer.ReadFloat();
		float ldz = deserializer.ReadFloat();
		Vector3 forceDirection = new Vector3(fdx, fdy, fdz);
		Vector3 lookDirection = new Vector3(ldx, ldy, ldz);

		Player holder = GetComponentInParent<Player>();
		if (holder == null || holder.refs?.ragdoll == null)
			return;

		Bodypart handR = holder.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
		Bodypart elbowR = holder.refs.ragdoll.GetBodypart(BodypartType.Elbow_R);
		Bodypart armR = holder.refs.ragdoll.GetBodypart(BodypartType.Arm_R);

		if (handR != null && elbowR != null && armR != null)
		{
			hitPlayers.Clear();
			isSwinging = true;
			StartCoroutine(PerformSwingReplicated(holder, handR, elbowR, armR, forceDirection, lookDirection));
		}
	}

	private void RPC_Hit(BinaryDeserializer deserializer)
	{
		int hitViewId = deserializer.ReadInt();
		float fx = deserializer.ReadFloat();
		float fy = deserializer.ReadFloat();
		float fz = deserializer.ReadFloat();
		Vector3 forceDirection = new Vector3(fx, fy, fz);

		PhotonView hitView = PhotonNetwork.GetPhotonView(hitViewId);
		if (hitView == null) return;

		Player hitPlayer = hitView.GetComponent<Player>();
		Player holder = GetComponentInParent<Player>();

		// Apply hit effects on all clients
		if (hitPlayer?.refs?.ragdoll != null)
		{
			hitPlayer.refs.ragdoll.TaseShock(3f);
			hitPlayer.refs.ragdoll.AddForce(forceDirection * 20f, ForceMode.VelocityChange);
		}

		// Visual/Audio effects on all clients
		if (batHitSFX)
			batHitSFX.Play(transform.position);

		if (isBreakable)
		{
			CreateBreakEffect(transform.position);

			// ONLY the owner handles inventory clearing and destruction
			// CHANGED: Use coroutine instead of immediate destruction to prevent NullReferenceExceptions
			if (isSimulatedByMe && holder != null)
			{
				StartCoroutine(DestroyBatAfterHit(holder));
			}
		}
	}

	/// <summary>
	/// ADDED: Handles bat destruction after a hit with proper cleanup sequence
	/// This prevents NullReferenceExceptions by:
	/// 1. Clearing the inventory slot first
	/// 2. Waiting 2 frames for the inventory system to update
	/// 3. Only then destroying the GameObject
	/// </summary>
	private IEnumerator DestroyBatAfterHit(Player holder)
	{
		// Step 1: Clear the inventory slot
		GlobalPlayerData globalPlayerData;
		if (GlobalPlayerData.TryGetPlayerData(holder.refs.view.Owner, out globalPlayerData))
		{
			PlayerInventory playerInventory = globalPlayerData.inventory;
			if (playerInventory != null && holder.data.selectedItemSlot >= 0)
			{
				InventorySlot slot;
				if (playerInventory.TryGetSlot(holder.data.selectedItemSlot, out slot))
				{
					slot.Clear();
				}
			}
		}

		// Step 2: Wait for the inventory system to process the slot clearing
		// This prevents the PlayerItems.Unequip() NullReferenceException
		yield return null; // Wait 1 frame
		yield return null; // Wait another frame for safety

		// Step 3: Now safely destroy the bat GameObject
		// Check if objects still exist before destroying (player might have disconnected)
		if (this != null && gameObject != null)
		{
			Destroy(gameObject);
		}
	}

	void Update()
	{
		if (!isInitialized || player == null) return;
		if (!this.isHeldByMe) return;
		if (player.HasLockedInput()) return;

		if (player.input.clickWasPressed && !isSwinging)
		{
			TriggerSwing();
		}
	}

	void TriggerSwing()
	{
		if (player == null || player.refs?.ragdoll == null || player.refs?.headPos == null)
			return;

		Vector3 lookDirection = player.refs.headPos.forward;
		Vector3 forceDirection = player.refs.headPos.forward;
		forceDirection += Vector3.down * 0.95f;
		forceDirection = forceDirection.normalized;

		BinarySerializer binarySerializer = new BinarySerializer();
		binarySerializer.WriteFloat(forceDirection.x);
		binarySerializer.WriteFloat(forceDirection.y);
		binarySerializer.WriteFloat(forceDirection.z);
		binarySerializer.WriteFloat(lookDirection.x);
		binarySerializer.WriteFloat(lookDirection.y);
		binarySerializer.WriteFloat(lookDirection.z);
		itemInstance.CallRPC(ItemRPC.RPC1, binarySerializer);
	}

	private IEnumerator PerformSwingReplicated(Player holder, Bodypart hand, Bodypart elbow, Bodypart arm, Vector3 forceDirection, Vector3 lookDirection)
	{
		// Add null checks for safety
		if (holder == null || hand == null || elbow == null || arm == null)
		{
			isSwinging = false;
			yield break;
		}

		Collider batCollider = GetComponentInChildren<Collider>(true);
		if (batCollider == null)
		{
			isSwinging = false;
			yield break;
		}

		float elapsed = 0f;

		while (elapsed < swingDuration)
		{
			// Null checks during swing (in case holder dies/disconnects)
			if (hand == null || hand.rig == null || 
			    elbow == null || elbow.rig == null || 
			    arm == null || arm.rig == null)
			{
				isSwinging = false;
				yield break;
			}

			float progress = elapsed / swingDuration;
			float forceCurve = Mathf.Sin(progress * Mathf.PI);
			float currentForce = swingForce * forceCurve;

			hand.rig.AddForce(forceDirection * currentForce, ForceMode.VelocityChange);
			elbow.rig.AddForce(forceDirection * currentForce * 0.6f, ForceMode.VelocityChange);
			arm.rig.AddForce(forceDirection * currentForce * 0.4f, ForceMode.VelocityChange);

			// Only the simulating client does hit detection to avoid duplicate RPCs
			if (isSimulatedByMe)
			{
				Collider[] hits = Physics.OverlapBox(
					batCollider.bounds.center,
					batCollider.bounds.extents,
					batCollider.transform.rotation,
					-1,
					QueryTriggerInteraction.Collide
				);

				foreach (Collider hit in hits)
					ProcessHit(hit, forceDirection, holder);
			}

			elapsed += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSeconds(cooldownTime - swingDuration);
		isSwinging = false;
	}

	private void ProcessHit(Collider other, Vector3 forceDirection, Player holder)
	{
		if (!isSwinging || other == null || holder == null) return;

		if (other.transform.root == holder?.transform.root)
			return;

		Player hitPlayer = other.GetComponentInParent<Player>();
		if (hitPlayer != null && hitPlayer != holder && !hitPlayers.Contains(hitPlayer))
		{
			hitPlayers.Add(hitPlayer);
			OnHitTarget(hitPlayer, forceDirection);
		}
	}

	private void OnHitTarget(Player hitPlayer, Vector3 forceDirection)
	{
		if (!isSimulatedByMe || lastFrame == Time.frameCount)
			return;
		lastFrame = Time.frameCount;

		// Null check before accessing ViewID
		if (hitPlayer?.refs?.view == null)
			return;

		BinarySerializer binarySerializer = new BinarySerializer();
		binarySerializer.WriteInt(hitPlayer.refs.view.ViewID);
		binarySerializer.WriteFloat(forceDirection.x);
		binarySerializer.WriteFloat(forceDirection.y);
		binarySerializer.WriteFloat(forceDirection.z);
		itemInstance.CallRPC(ItemRPC.RPC0, binarySerializer);
	}

	/// <summary>
	/// Creates a particle effect showing the bat breaking into pieces
	/// </summary>
	private void CreateBreakEffect(Vector3 position)
	{
		// Create a temporary GameObject for the particle system
		GameObject particleObj = new GameObject("BatBreakEffect");
		particleObj.transform.position = position;

		// Add ParticleSystem component
		ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

		// Configure main module - EXPLOSIVE SETTINGS
		var main = ps.main;
		main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
		main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f);
		main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
		main.startColor = new Color(0.6f, 0.4f, 0.2f);
		main.maxParticles = 100;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.duration = 0.3f;
		main.loop = false;
		main.gravityModifier = 1.5f;

		// Configure emission
		var emission = ps.emission;
		emission.rateOverTime = 0;
		emission.SetBursts(new ParticleSystem.Burst[] {
			new ParticleSystem.Burst(0f, 60, 100, 1, 0f)
		});

		// Configure shape
		var shape = ps.shape;
		shape.shapeType = ParticleSystemShapeType.Sphere;
		shape.radius = 0.05f;
		shape.radiusThickness = 0f;

		// Size over lifetime
		var sizeOverLifetime = ps.sizeOverLifetime;
		sizeOverLifetime.enabled = true;
		sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

		// Color over lifetime
		var colorOverLifetime = ps.colorOverLifetime;
		colorOverLifetime.enabled = true;
		Gradient grad = new Gradient();
		grad.SetKeys(
			new GradientColorKey[] {
				new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0f),
				new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 1f)
			},
			new GradientAlphaKey[] {
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(0f, 1f)
			}
		);
		colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

		// Add renderer
		var renderer = ps.GetComponent<ParticleSystemRenderer>();
		renderer.renderMode = ParticleSystemRenderMode.Billboard;

		Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
		particleMaterial.color = new Color(0.6f, 0.4f, 0.2f, 1f);
		renderer.material = particleMaterial;

		ps.Play();

		Destroy(particleObj, main.duration + main.startLifetime.constantMax + 0.5f);
	}
}
