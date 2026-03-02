using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MonsterAffectingAOE : MonoBehaviour
{
	public bool doOnStart = true;

	public float damage = 150f;

	public float force;

	public float fall = 2f;

	public float radius = 8f;

	public float innerRadius = 4f;

	private void Start()
	{
		if (doOnStart)
		{
			DoAOE();
		}
	}

	private void DoAOE()
	{
		DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: DoAOE");
		List<Player> list = new List<Player>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		foreach (Collider collider in array)
		{
			if ((bool)collider.attachedRigidbody)
			{
				Player componentInParent = collider.GetComponentInParent<Player>();
				if (!list.Contains(componentInParent) && componentInParent != null)
				{
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: player found");

					list.Add(componentInParent);
					float value = Vector3.Distance(base.transform.position, collider.transform.position);
					// log base position and collider position
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: base position: " + base.transform.position);
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: collider position: " + collider.transform.position);
					// log radius, innerradius and value
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: radius: " + radius + " innerRadius: " + innerRadius + " value: " + value);
					float num = Mathf.InverseLerp(radius, innerRadius, value);
					float aiDmgMultiplier = componentInParent.ai ? 0f : 1f;
					float aiFallMultiplier = componentInParent.ai ? 1.6f : 1f;
					Vector3 vector = (componentInParent.Center() - base.transform.position).normalized * num * force;
					// log ai multipliers
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: aiDmgMultiplier: " + aiDmgMultiplier);
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: aiFallMultiplier: " + aiFallMultiplier);

					var finalDamage = damage * num * aiDmgMultiplier;
					var finalFall = fall * num * aiFallMultiplier;

					// log damage, num, and aimultiplier in one line
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: damage: " + damage + " num: " + num + " aiDmgMultiplier: " + aiDmgMultiplier);
					// fall num and ai mult
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: fall: " + fall + " num: " + num + " aiFallMultiplier: " + aiFallMultiplier);

					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: finalDamage: " + finalDamage);
					DbsContentApi.Modules.Logger.Log("MonsterAffectingAOE: finalFall: " + finalFall);

					componentInParent.CallTakeDamageAndAddForceAndFall(finalDamage, vector, finalFall);
				}
			}
		}
	}
}
