using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to the invisible player's GameObject (or a child).
/// Set playerName and actorNumber from the local player before the camera polls.
/// </summary>
public class InvisiblePlayerContentProvider : ContentProvider
{
	public string playerName = string.Empty;
	public int actorNumber;

	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(
			new InvisiblePlayerContentEvent(playerName, actorNumber, transform.position),
			seenAmount, time));
	}
}
