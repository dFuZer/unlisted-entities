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
	private Coroutine currentSwingCoroutine = null;
	
	private static HashSet<string> registeredItems = new HashSet<string>();

	public float swingForce = 3f;
	public float swingDuration = 0.6f;
	public float cooldownTime = 1.6f;

	public SFX_Instance batHitSFX = null!;
	public bool isBreakable;

	private HashSet<Player> hitPlayers = new HashSet<Player>();

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		Debug.Log($"[BAT] ConfigItem called for {gameObject.name}");
		player = GetComponentInParent<Player>();
		
		string itemId = itemInstance.GetInstanceID().ToString();
		
		if (!registeredItems.Contains(itemId))
		{
			itemInstance.RegisterRPC(ItemRPC.RPC0, RPC_Hit);
			itemInstance.RegisterRPC(ItemRPC.RPC1, RPC_StartSwing);
			registeredItems.Add(itemId);
			Debug.Log($"[BAT] Registered RPCs for item {itemId}");
		}
		
		isInitialized = true;
		
		// Reset swing state
		if (currentSwingCoroutine != null)
		{
			Debug.Log($"[BAT] Stopping existing coroutine in ConfigItem");
			StopCoroutine(currentSwingCoroutine);
			currentSwingCoroutine = null;
		}
		isSwinging = false;
		Debug.Log($"[BAT] ConfigItem complete. isSwinging={isSwinging}");
	}

	private void RPC_StartSwing(BinaryDeserializer deserializer)
	{
		Debug.Log($"[BAT] RPC_StartSwing called. isSwinging={isSwinging}");
		
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
		{
			Debug.LogWarning($"[BAT] RPC_StartSwing: holder or ragdoll is null");
			return;
		}

		Bodypart handR = holder.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
		Bodypart elbowR = holder.refs.ragdoll.GetBodypart(BodypartType.Elbow_R);
		Bodypart armR = holder.refs.ragdoll.GetBodypart(BodypartType.Arm_R);

		if (handR != null && elbowR != null && armR != null)
		{
			hitPlayers.Clear();
			isSwinging = true;
			
			if (currentSwingCoroutine != null)
			{
				Debug.Log($"[BAT] Stopping previous swing coroutine");
				StopCoroutine(currentSwingCoroutine);
			}
			
			currentSwingCoroutine = StartCoroutine(PerformSwingReplicated(holder, handR, elbowR, armR, forceDirection, lookDirection));
			Debug.Log($"[BAT] Started swing coroutine. isSwinging={isSwinging}");
		}
		else
		{
			Debug.LogWarning($"[BAT] RPC_StartSwing: Missing bodyparts");
		}
	}

	private void RPC_Hit(BinaryDeserializer deserializer)
	{
		Debug.Log($"[BAT] RPC_Hit called");
		
		int hitViewId = deserializer.ReadInt();
		float fx = deserializer.ReadFloat();
		float fy = deserializer.ReadFloat();
		float fz = deserializer.ReadFloat();
		Vector3 forceDirection = new Vector3(fx, fy, fz);

		PhotonView hitView = PhotonNetwork.GetPhotonView(hitViewId);
		if (hitView == null) return;

		Player hitPlayer = hitView.GetComponent<Player>();
		Player holder = GetComponentInParent<Player>();

		if (hitPlayer?.refs?.ragdoll != null)
		{
			hitPlayer.refs.ragdoll.TaseShock(3f);
			hitPlayer.refs.ragdoll.AddForce(forceDirection * 20f, ForceMode.VelocityChange);
		}

		if (batHitSFX)
			batHitSFX.Play(transform.position);

		if (isBreakable)
		{
			CreateBreakEffect(transform.position);

			if (isSimulatedByMe && holder != null)
			{
				Debug.Log($"[BAT] Starting destroy sequence");
				StartCoroutine(DestroyBatAfterHit(holder));
			}
		}
	}

	private IEnumerator DestroyBatAfterHit(Player holder)
	{
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

		yield return null;
		yield return null;

		string itemId = itemInstance.GetInstanceID().ToString();
		registeredItems.Remove(itemId);

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
			Debug.Log($"[BAT] Click detected. Triggering swing. isSwinging={isSwinging}");
			TriggerSwing();
		}
		else if (player.input.clickWasPressed && isSwinging)
		{
			Debug.Log($"[BAT] Click detected but already swinging. isSwinging={isSwinging}");
		}
	}

	void TriggerSwing()
	{
		if (player == null || player.refs?.ragdoll == null || player.refs?.headPos == null)
		{
			Debug.LogWarning($"[BAT] TriggerSwing: Missing player refs");
			return;
		}

		Vector3 lookDirection = player.refs.headPos.forward;
		Vector3 forceDirection = player.refs.headPos.forward;
		forceDirection += Vector3.down * 0.95f;
		forceDirection = forceDirection.normalized;

		Debug.Log($"[BAT] Calling RPC_StartSwing via RPC");
		
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
		Debug.Log($"[BAT] PerformSwingReplicated started. Duration={swingDuration}, Cooldown={cooldownTime}");
		
		if (holder == null || hand == null || elbow == null || arm == null)
		{
			Debug.LogWarning($"[BAT] PerformSwingReplicated: Null bodyparts detected");
			isSwinging = false;
			currentSwingCoroutine = null;
			yield break;
		}

		Collider batCollider = GetComponentInChildren<Collider>(true);
		if (batCollider == null)
		{
			Debug.LogWarning($"[BAT] PerformSwingReplicated: No collider found");
			isSwinging = false;
			currentSwingCoroutine = null;
			yield break;
		}

		float elapsed = 0f;

		while (elapsed < swingDuration)
		{
			if (hand == null || hand.rig == null || 
			    elbow == null || elbow.rig == null || 
			    arm == null || arm.rig == null)
			{
				Debug.LogWarning($"[BAT] Bodypart became null during swing");
				isSwinging = false;
				currentSwingCoroutine = null;
				yield break;
			}

			float progress = elapsed / swingDuration;
			float forceCurve = Mathf.Sin(progress * Mathf.PI);
			float currentForce = swingForce * forceCurve;

			hand.rig.AddForce(forceDirection * currentForce, ForceMode.VelocityChange);
			elbow.rig.AddForce(forceDirection * currentForce * 0.6f, ForceMode.VelocityChange);
			arm.rig.AddForce(forceDirection * currentForce * 0.4f, ForceMode.VelocityChange);

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

		Debug.Log($"[BAT] Swing animation complete. Starting cooldown...");
		
		float remainingCooldown = Mathf.Max(0f, cooldownTime - swingDuration);
		Debug.Log($"[BAT] Remaining cooldown: {remainingCooldown}s");
		
		if (remainingCooldown > 0f)
		{
			yield return new WaitForSeconds(remainingCooldown);
		}
		
		isSwinging = false;
		currentSwingCoroutine = null;
		Debug.Log($"[BAT] Swing complete. isSwinging={isSwinging}");
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
			Debug.Log($"[BAT] Hit detected on player {hitPlayer.refs.view.ViewID}");
			OnHitTarget(hitPlayer, forceDirection);
		}
	}

	private void OnHitTarget(Player hitPlayer, Vector3 forceDirection)
	{
		if (!isSimulatedByMe || lastFrame == Time.frameCount)
			return;
		lastFrame = Time.frameCount;

		if (hitPlayer?.refs?.view == null)
			return;

		Debug.Log($"[BAT] Sending RPC_Hit for player {hitPlayer.refs.view.ViewID}");
		
		BinarySerializer binarySerializer = new BinarySerializer();
		binarySerializer.WriteInt(hitPlayer.refs.view.ViewID);
		binarySerializer.WriteFloat(forceDirection.x);
		binarySerializer.WriteFloat(forceDirection.y);
		binarySerializer.WriteFloat(forceDirection.z);
		itemInstance.CallRPC(ItemRPC.RPC0, binarySerializer);
	}

	private void OnDisable()
	{
		Debug.Log($"[BAT] OnDisable called. isSwinging={isSwinging}");
		isSwinging = false;
		if (currentSwingCoroutine != null)
		{
			StopCoroutine(currentSwingCoroutine);
			currentSwingCoroutine = null;
		}
	}
	
	private void OnDestroy()
	{
		Debug.Log($"[BAT] OnDestroy called");
		if (itemInstance != null)
		{
			string itemId = itemInstance.GetInstanceID().ToString();
			registeredItems.Remove(itemId);
		}
	}

	private void CreateBreakEffect(Vector3 position)
	{
		GameObject particleObj = new GameObject("BatBreakEffect");
		particleObj.transform.position = position;

		ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

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

		var emission = ps.emission;
		emission.rateOverTime = 0;
		emission.SetBursts(new ParticleSystem.Burst[] {
			new ParticleSystem.Burst(0f, 60, 100, 1, 0f)
		});

		var shape = ps.shape;
		shape.shapeType = ParticleSystemShapeType.Sphere;
		shape.radius = 0.05f;
		shape.radiusThickness = 0f;

		var sizeOverLifetime = ps.sizeOverLifetime;
		sizeOverLifetime.enabled = true;
		sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

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

		var renderer = ps.GetComponent<ParticleSystemRenderer>();
		renderer.renderMode = ParticleSystemRenderMode.Billboard;

		Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
		particleMaterial.color = new Color(0.6f, 0.4f, 0.2f, 1f);
		renderer.material = particleMaterial;

		ps.Play();

		Destroy(particleObj, main.duration + main.startLifetime.constantMax + 0.5f);
	}
}