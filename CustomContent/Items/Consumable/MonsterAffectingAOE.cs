using System;
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

	/// <summary>
	/// Optional callback when an AI/monster player is affected (master client only).
	/// </summary>
	public Action<Player, Vector3>? onMonsterHit;

	private void Start()
	{
		if (doOnStart)
		{
			DoAOE();
		}
	}

	private void DoAOE()
	{
		Logger.Log("MonsterAffectingAOE: DoAOE");
		List<Player> list = new List<Player>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		foreach (Collider collider in array)
		{
			if ((bool)collider.attachedRigidbody)
			{
				Player componentInParent = collider.GetComponentInParent<Player>();
				if (!list.Contains(componentInParent) && componentInParent != null)
				{
					Logger.Log("MonsterAffectingAOE: player found");

					list.Add(componentInParent);
					float value = Vector3.Distance(base.transform.position, collider.transform.position);
					// log base position and collider position
					Logger.Log("MonsterAffectingAOE: base position: " + base.transform.position);
					Logger.Log("MonsterAffectingAOE: collider position: " + collider.transform.position);
					// log radius, innerradius and value
					Logger.Log("MonsterAffectingAOE: radius: " + radius + " innerRadius: " + innerRadius + " value: " + value);
					float num = Mathf.InverseLerp(radius, innerRadius, value);
					float aiDmgMultiplier = componentInParent.ai ? 0f : 1f;
					float aiFallMultiplier = componentInParent.ai ? 1.6f : 1f;
					Vector3 vector = (componentInParent.Center() - base.transform.position).normalized * num * force;
					// log ai multipliers
					Logger.Log("MonsterAffectingAOE: aiDmgMultiplier: " + aiDmgMultiplier);
					Logger.Log("MonsterAffectingAOE: aiFallMultiplier: " + aiFallMultiplier);

					var finalDamage = damage * num * aiDmgMultiplier;
					var finalFall = fall * num * aiFallMultiplier;

					// log damage, num, and aimultiplier in one line
					Logger.Log("MonsterAffectingAOE: damage: " + damage + " num: " + num + " aiDmgMultiplier: " + aiDmgMultiplier);
					// fall num and ai mult
					Logger.Log("MonsterAffectingAOE: fall: " + fall + " num: " + num + " aiFallMultiplier: " + aiFallMultiplier);

					Logger.Log("MonsterAffectingAOE: finalDamage: " + finalDamage);
					Logger.Log("MonsterAffectingAOE: finalFall: " + finalFall);

					componentInParent.CallTakeDamageAndAddForceAndFall(finalDamage, vector, finalFall);

					if (componentInParent.ai && PhotonNetwork.IsMasterClient)
						onMonsterHit?.Invoke(componentInParent, transform.position);
				}
			}
		}
	}
}
