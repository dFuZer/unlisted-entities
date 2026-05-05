using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to a short-lived trigger object at the hit location.
/// Set playerName and actorNumber from the hit player before the camera polls.
/// </summary>
public class TranqGunAllyContentProvider : ContentProvider
{
	public string playerName = string.Empty;
	public int actorNumber;

	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(
			new TranqGunAllyContentEvent(playerName, actorNumber, transform.position),
			seenAmount, time));
	}
}
