using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to a short-lived trigger object at the explosion location.
/// Set playerName and actorNumber from the hit player before the camera polls.
/// </summary>
public class ElectricGrenadeAllyContentProvider : ContentProvider
{
	public string playerName = string.Empty;
	public int actorNumber;

	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(
			new ElectricGrenadeAllyContentEvent(playerName, actorNumber, transform.position),
			seenAmount, time));
	}
}
