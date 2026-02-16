using System.Collections.Generic;
using UnityEngine;

public class ElectricGrenadeExplosionAOE : MonoBehaviour
{
	public float damage = 20;

	public float force = 4f;

	public float fall = 6f;

	public float radius = 8f;

	public float innerRadius = 4f;

	private void Start()
	{
		List<Player> list = new List<Player>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		foreach (Collider collider in array)
		{
			if ((bool)collider.attachedRigidbody)
			{
				Player componentInParent = collider.GetComponentInParent<Player>();
				if ((bool)componentInParent && componentInParent.refs.view.IsMine && !list.Contains(componentInParent))
				{
					list.Add(componentInParent);
					float value = Vector3.Distance(base.transform.position, collider.transform.position);
					float num = Mathf.InverseLerp(radius, innerRadius, value);
					Vector3 vector = (componentInParent.Center() - base.transform.position).normalized * num * force;
					componentInParent.CallTakeDamageAndAddForceAndFall(damage * num, vector, fall * num);
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
