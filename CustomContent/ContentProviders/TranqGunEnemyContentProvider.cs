using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to a short-lived trigger object at the hit location.
/// Set targetViewID to the enemy's PhotonView.ViewID before the camera polls.
/// </summary>
public class TranqGunEnemyContentProvider : ContentProvider
{
	public int targetViewID;

	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(new TranqGunEnemyContentEvent
		{
			viewID = targetViewID,
			worldPosition = transform.position,
		}, seenAmount, time));
	}
}
