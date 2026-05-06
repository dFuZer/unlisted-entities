using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using DbsContentApi.Modules.Utility;
using UnlistedEntities.CustomContent;
using UnlistedEntities.CustomContent.ContentEvents;

public class ElectricGrenadeExplosionAOE : MonoBehaviour
{
	public float damage = 20;

	public float force = 4f;

	public float fall = 6f;

	public float radius = 8f;

	public float innerRadius = 4f;

	private void Start()
	{
		DbsContentApi.Modules.Logger.Log("ElectricGrenadeExplosionAOE: start");
		List<Player> list = new List<Player>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		foreach (Collider collider in array)
		{
			if ((bool)collider.attachedRigidbody)
			{
				Player componentInParent = collider.GetComponentInParent<Player>();
				if ((bool)componentInParent && !list.Contains(componentInParent))
				{
					if (componentInParent.refs?.view == null)
					{
						DbsContentApi.Modules.Logger.LogError("ElectricGrenadeExplosionAOE: overlapped Player has null refs or PhotonView; skipping damage and electric grenade content for this overlap.");
						continue;
					}

					DbsContentApi.Modules.Logger.Log("ElectricGrenadeExplosionAOE: player found");
					list.Add(componentInParent);
					float value = Vector3.Distance(base.transform.position, collider.transform.position);
					float num = Mathf.InverseLerp(radius, innerRadius, value);
					var aiForceMultiplier = componentInParent.ai ? 1.6f : 1f;
					var aiFallMultiplier = componentInParent.ai ? 1.6f : 1f;
					var aiDmgMultiplier = componentInParent.ai ? 0f : 1f;
					Vector3 vector = (componentInParent.Center() - base.transform.position).normalized * num * force * aiForceMultiplier;

					if (componentInParent.refs.view.IsMine || (PhotonNetwork.IsMasterClient && componentInParent.ai))
					{
						componentInParent.CallTakeDamageAndAddForceAndFall(damage * num * aiDmgMultiplier, vector, fall * num * aiFallMultiplier);

						if (CustomItems.TemporaryContentTriggerPrefab == null)
						{
							DbsContentApi.Modules.Logger.LogError("ElectricGrenadeExplosionAOE: TemporaryContentTriggerPrefab is null; electric grenade content provider not spawned.");
						}
						else
						{
							GameObject trigger = ObjectHelper.CreateTemporaryTriggerObject(50, CustomItems.TemporaryContentTriggerPrefab);
							if (trigger == null)
							{
								DbsContentApi.Modules.Logger.LogError("ElectricGrenadeExplosionAOE: CreateTemporaryTriggerObject returned null.");
							}
							else
							{
								trigger.transform.position = transform.position;
								if (componentInParent.ai)
								{
									var provider = trigger.AddComponent<ElectricGrenadeMonsterContentProvider>();
								}
								else if (componentInParent.refs.view.Owner != null)
								{
									var provider = trigger.AddComponent<ElectricGrenadeAllyContentProvider>();
								}
								else
								{
									DbsContentApi.Modules.Logger.LogError("ElectricGrenadeExplosionAOE: hit human Player has no PhotonView.Owner; ElectricGrenadeAllyContentProvider not configured.");
									Object.Destroy(trigger);
								}
							}
						}
					}

					if (componentInParent.TryGetInventory(out var inv))
					{
						const float rechargeFraction = 0.4f;
						foreach (var slot in inv.slots)
						{
							if (slot.ItemInSlot.item != null && slot.ItemInSlot.data.TryGetEntry<BatteryEntry>(out var battery) && battery.m_charge < battery.m_maxCharge)
							{
								battery.AddCharge(rechargeFraction * battery.m_maxCharge);
							}
						}
					}
				}
			}
		}
	}
}
