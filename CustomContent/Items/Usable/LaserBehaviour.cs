using UnityEngine;
using Photon.Pun;
using System;

/// <summary>
/// Custom item behaviour with laser beam that kills players on contact
/// </summary>
public class LaserBehaviour : ItemInstanceBehaviour
{
	public Light itemLight;
 Laser laser;
	private LineRenderer laserLineRenderer;
	
	private BatteryEntry batteryEntry;
	private OnOffEntry onOffEntry;
	private bool isInitialized = false;

	// Laser firing control
	private float laserFireTimer = 0f;
	private const float LASER_FIRE_DURATION = 0.03f; // 30ms
	private bool laserCanFire = true;

	public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
	{
		Debug.Log("ConfigItem called for LaserBehaviour");

		GameObject itemGameObject = this.gameObject;
		
		// Add the Laser component to the item instance
		laser = itemGameObject.GetComponent<Laser>();
		if (laser == null)
		{
			laser = itemGameObject.AddComponent<Laser>();
			Debug.Log("Added Laser component to item instance");
		}

		// Set laser to live mode (updates every frame)
		laser.liveLaser = true;

		// Create the laser beam child object with visual components
		GameObject beamOrigin = new GameObject("LaserBeam");
		beamOrigin.transform.SetParent(itemGameObject.transform);
		beamOrigin.transform.localPosition = Vector3.forward * 0.2f;
		beamOrigin.transform.localRotation = Quaternion.identity;

		// Add LineRenderer for visible laser beam
		laserLineRenderer = beamOrigin.AddComponent<LineRenderer>();
		laserLineRenderer.startWidth = 0.05f;
		laserLineRenderer.endWidth = 0.05f;
		laserLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		laserLineRenderer.startColor = Color.red;
		laserLineRenderer.endColor = Color.red;
		laserLineRenderer.positionCount = 2;
		laserLineRenderer.SetPosition(0, Vector3.zero);
		laserLineRenderer.SetPosition(1, Vector3.forward);
		laserLineRenderer.useWorldSpace = false;
		laserLineRenderer.enabled = false; // Start disabled

		Debug.Log("Created LaserBeam with LineRenderer");

		// Ensure data is not null
		if (data == null)
		{
			Debug.LogError("ItemInstanceData is null in ConfigItem!");
			return;
		}

		try
		{
			// Battery (persistent across drops/rounds)
			if (!data.TryGetEntry(out batteryEntry))
			{
				batteryEntry = new BatteryEntry
				{
					m_charge = 100f,
					m_maxCharge = 100f
				};
				data.AddDataEntry(batteryEntry);
				Debug.Log("Created new BatteryEntry");
			}

			// On/Off state (networked)
			if (!data.TryGetEntry(out onOffEntry))
			{
				onOffEntry = new OnOffEntry();
				data.AddDataEntry(onOffEntry);
				Debug.Log("Created new OnOffEntry");
			}

			// Find or create Light component
			if (itemLight == null)
			{
				itemLight = GetComponentInChildren<Light>();
				if (itemLight == null)
				{
					Debug.Log("Creating new Light component");
					GameObject lightObj = new GameObject("ItemLight");
					lightObj.transform.SetParent(transform);
					lightObj.transform.localPosition = Vector3.zero;
					itemLight = lightObj.AddComponent<Light>();
					itemLight.type = LightType.Point;
					itemLight.range = 10f;
					itemLight.intensity = 100f;
					itemLight.color = Color.red;
				}
			}

			// Update visuals to match state
			if (itemLight != null && onOffEntry != null)
			{
				itemLight.enabled = onOffEntry.on;
			}

			isInitialized = true;
			Debug.Log("LaserBehaviour initialized successfully");
		}
		catch (Exception e)
		{
			Debug.LogError($"Error in ConfigItem: {e.Message}\n{e.StackTrace}");
		}
	}

	private void Update()
	{
		// Wait until properly initialized
		if (!isInitialized)
			return;

		// Safety checks
		if (itemLight == null || batteryEntry == null || onOffEntry == null)
		{
			Debug.LogWarning("LaserBehaviour: Missing component in Update");
			return;
		}

		// Only allow local player to control
		if (itemInstance == null)
			return;

		// Check if player has locked input
		if (Player.localPlayer != null && Player.localPlayer.HasLockedInput())
			return;

		// Left click: fire laser for 30ms
		if (this.isHeldByMe && !Player.localPlayer.HasLockedInput()
			&& Player.localPlayer.input.clickWasPressed
			&& laserCanFire && batteryEntry.m_charge > 0f)
		{
			try
			{
				onOffEntry.on = true;
				onOffEntry.SetDirty();
				laserFireTimer = LASER_FIRE_DURATION;
				laserCanFire = false;
				Debug.Log("Laser fired!");
			}
			catch (Exception e)
			{
				Debug.LogError($"Error firing laser: {e.Message}");
			}
		}

		// Handle laser fire timer
		if (laserFireTimer > 0f)
		{
			laserFireTimer -= Time.deltaTime;
			
			// Check for player hits while laser is active
			CheckForPlayerHits();
			
			if (laserFireTimer <= 0f)
			{
				onOffEntry.on = false;
				onOffEntry.SetDirty();
			}
		}

		// Reset fire capability when mouse button is released
		if (Input.GetMouseButtonUp(0))
		{
			laserCanFire = true;
		}

		// Battery drain while on
		if (onOffEntry.on)
		{
			batteryEntry.m_charge -= Time.deltaTime * 20f; // Faster drain for burst fire
			batteryEntry.m_charge = Mathf.Max(0f, batteryEntry.m_charge);
			
			if (batteryEntry.m_charge <= 0f)
			{
				onOffEntry.on = false;
				onOffEntry.SetDirty();
				laserFireTimer = 0f;
				Debug.Log("Battery depleted!");
			}
		}

		// Update light visual
		bool isActive = onOffEntry.on && batteryEntry.m_charge > 0f;
		itemLight.enabled = isActive;

		// Update laser beam visual
		if (laser != null && laserLineRenderer != null)
		{
			Transform beamTransform = transform.Find("LaserBeam");
			if (beamTransform != null)
			{
				// Enable/disable laser beam based on on/off state
				laserLineRenderer.enabled = isActive;
				
				// Update the line length based on the beam scale (set by Laser component)
				float distance = beamTransform.localScale.z;
				laserLineRenderer.SetPosition(1, Vector3.forward * distance);
			}
		}
	}

	private void CheckForPlayerHits()
	{
		Transform beamTransform = transform.Find("LaserBeam");
		if (beamTransform == null)
			return;

		// Get the laser's current hit distance
		float distance = beamTransform.localScale.z;
		
		// Raycast to check for players and monsters
		Vector3 startPos = transform.position + transform.forward * 0.2f;
		Vector3 direction = transform.forward;
		
		RaycastHit[] hits = Physics.RaycastAll(startPos, direction, distance);
		
		foreach (RaycastHit hit in hits)
		{
			// Check if we hit a player
			Player player = hit.collider.GetComponentInParent<Player>();
			if (player != null && player != Player.localPlayer)
			{
				// Shock the player (same as shock stick)
				Debug.Log($"Laser hit player: {player.name}");
				player.refs.ragdoll.TaseShock(5f);
				
				// Add screen shake for local player
				if (player.refs.view.IsMine)
				{
					GamefeelHandler.instance.perlin.AddShake(4f, 0.3f, 20f);
				}
				
				return; // Only hit one target per burst
			}

			// Check if we hit a bot/monster - try to find any ragdoll component
			Bot bot = hit.collider.GetComponentInParent<Bot>();
			if (bot != null)
			{
				Debug.Log($"Laser hit bot: {bot.name}");
				
				// Try to find PlayerRagdoll component (some bots might use this)
				var ragdoll = bot.GetComponentInChildren<PlayerRagdoll>();
				if (ragdoll != null)
				{
					Debug.Log("Applied ragdoll shock to bot");
					ragdoll.TaseShock(5f);
				}
				else
				{
					// If no ragdoll found, try to add force to make it stumble
					Rigidbody[] rigidbodies = bot.GetComponentsInChildren<Rigidbody>();
					foreach (var rb in rigidbodies)
					{
						rb.AddForce(direction * 10f, ForceMode.VelocityChange);
					}
					Debug.Log("No ragdoll found, applied force to bot");
				}
				
				return; // Only hit one target per burst
			}
		}
	}
}