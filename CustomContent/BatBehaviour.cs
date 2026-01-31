using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class BatBehaviour : ItemInstanceBehaviour
{
	private Player player;
	private bool isSwinging = false;
	private bool isInitialized = false;

	// Swing parameters - GENTLE to prevent self-knockdown
	public float swingForce = 3f;
	public float swingDuration = 0.6f;
	public float cooldownTime = 0.8f;

	public SFX_Instance batHitSFX = null!;

	// Collision tracking
	private HashSet<Player> hitPlayers = new HashSet<Player>();

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{

		player = GetComponentInParent<Player>();

		isInitialized = true;
	}

	void Update()
	{
		if (!isInitialized || player == null) return;

		if (!this.isHeldByMe) return;
		if (Player.localPlayer != null && Player.localPlayer.HasLockedInput()) return;

		if (Player.localPlayer.input.clickWasPressed && !isSwinging)
		{
			TriggerSwing();
		}
	}

	void TriggerSwing()
	{
		hitPlayers.Clear();
		PerformSwingLocal();
	}

	private void PerformSwingLocal()
	{
		if (player == null || player.refs?.ragdoll == null)
		{
			return;
		}

		isSwinging = true;

		Bodypart handR = player.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
		Bodypart elbowR = player.refs.ragdoll.GetBodypart(BodypartType.Elbow_R);
		Bodypart armR = player.refs.ragdoll.GetBodypart(BodypartType.Arm_R);

		if (handR != null && elbowR != null && armR != null)
		{
			StartCoroutine(PerformSwing(handR, elbowR, armR));
		}
		else
		{
			isSwinging = false;
		}
	}

	private IEnumerator PerformSwing(Bodypart hand, Bodypart elbow, Bodypart arm)
	{
		Vector3 lookDirection = player.refs.headPos.forward;
		Vector3 forceDirection = player.refs.headPos.forward;

		// Add strong downward component for baseball-style swing
		forceDirection += Vector3.down * 0.95f;
		forceDirection = forceDirection.normalized;

		Vector3 swingDirection = (lookDirection + player.refs.headPos.right * 0.01f).normalized;
		Collider batCollider = GetComponentInChildren<Collider>(true);
		if (batCollider == null)
		{
			isSwinging = false;
			yield break;
		}

		float elapsed = 0f;

		// SWING LOOP WITH MANUAL HIT DETECTION
		while (elapsed < swingDuration)
		{
			// Gentle arm swing forces
			float progress = elapsed / swingDuration;
			float forceCurve = Mathf.Sin(progress * Mathf.PI);
			float currentForce = swingForce * forceCurve;

			hand.rig.AddForce(forceDirection * currentForce, ForceMode.VelocityChange);
			elbow.rig.AddForce(forceDirection * currentForce * 0.6f, ForceMode.VelocityChange);
			arm.rig.AddForce(forceDirection * currentForce * 0.4f, ForceMode.VelocityChange);

			// MANUAL HIT DETECTION using bat collider bounds
			Collider[] hits = Physics.OverlapBox(
				batCollider.bounds.center,
				batCollider.bounds.extents,
				batCollider.transform.rotation,
				-1,
				QueryTriggerInteraction.Collide
			);

			foreach (Collider hit in hits)
			{
				ProcessHit(hit, lookDirection);
			}

			elapsed += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		// Cooldown
		yield return new WaitForSeconds(cooldownTime - swingDuration);
		isSwinging = false;
	}

	private void ProcessHit(Collider other, Vector3 forceDirection)
	{
		if (!isSwinging) return;

		// Ignore self
		if (other.transform.root == player?.transform.root)
		{
			return;
		}

		// Find target
		Player hitPlayer = other.GetComponentInParent<Player>();
		if (hitPlayer != null && hitPlayer != Player.localPlayer && !hitPlayers.Contains(hitPlayer))
		{
			hitPlayers.Add(hitPlayer);
			OnHitTarget(hitPlayer, forceDirection);
		}
	}

	private void OnHitTarget(Player hitPlayer, Vector3 forceDirection)
	{

		if (hitPlayer.refs?.ragdoll != null)
		{
			hitPlayer.refs.ragdoll.TaseShock(1f);
			hitPlayer.refs.ragdoll.AddForce(forceDirection * 20f, ForceMode.VelocityChange);
		}

		// Create breaking particle effect at bat position
		CreateBreakEffect(transform.position);
		if (batHitSFX)
			batHitSFX.Play(transform.position);
		// Remove the bat from inventory
		GlobalPlayerData globalPlayerData;
		if (GlobalPlayerData.TryGetPlayerData(player.refs.view.Owner, out globalPlayerData))
		{
			PlayerInventory playerInventory = globalPlayerData.inventory;
			if (playerInventory != null && player.data.selectedItemSlot >= 0)
			{
				// Get the slot and clear it properly
				InventorySlot slot;
				if (playerInventory.TryGetSlot(player.data.selectedItemSlot, out slot))
				{
					slot.Clear(); // This syncs across network via RPC
				}
			}
		}
		Destroy(this.gameObject);
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
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f); // Variable lifetime
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f); // Much faster, variable speed for explosion effect
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f); // Smaller particles
        main.startColor = new Color(0.6f, 0.4f, 0.2f); // Brown wood color
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.duration = 0.3f;
        main.loop = false;
        main.gravityModifier = 1.5f; // Use gravity modifier instead of velocity over lifetime

        // Configure emission - bigger burst for explosion
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 60, 100, 1, 0f) // More particles
        });

        // Configure shape - sphere emit in ALL directions
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f; // Small radius for point source
        shape.radiusThickness = 0f; // Emit from surface only

        // Size over lifetime - shrink as they fade
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        // Color over lifetime - fade out
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
        
        // Try to find a simple material or create a basic one
        Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMaterial.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown wood color
        renderer.material = particleMaterial;

        // Play the particle system
        ps.Play();

        // Destroy the particle object after it's done
        Destroy(particleObj, main.duration + main.startLifetime.constantMax + 0.5f);

	}
}