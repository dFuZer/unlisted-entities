using System.Collections.Generic;
using UnityEngine;

public class MonsterAffectingAOE : MonoBehaviour
{
	public bool doOnStart = true;

	public float damage = 150f;

	public float force = 4f;

	public float fall = 3f;

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
				if ((bool)componentInParent && (componentInParent.refs.view.IsMine || componentInParent.ai) && !list.Contains(componentInParent))
				{
					list.Add(componentInParent);
					float value = Vector3.Distance(base.transform.position, collider.transform.position);
					float num = Mathf.InverseLerp(radius, innerRadius, value);
					var aiDmgMultiplier = componentInParent.ai ? 0f : 1f;
					var aiForceMultiplier = componentInParent.ai ? 1.6f : 1f;
					var aiFallMultiplier = componentInParent.ai ? 1.6f : 1f;
					Vector3 vector = (componentInParent.Center() - base.transform.position).normalized * num * force * aiForceMultiplier;
					componentInParent.CallTakeDamageAndAddForceAndFall(damage * num * aiDmgMultiplier, vector, fall * num * aiFallMultiplier);
				}
			}
		}
	}
}
