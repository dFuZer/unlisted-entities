using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to the player wearing angel wings (or a child of them).
/// Set playerName and actorNumber from the wearing player before the camera polls.
/// </summary>
public class AngelWingsContentProvider : ContentProvider
{
	public string playerName = string.Empty;
	public int actorNumber;

	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(
			new AngelWingsContentEvent(playerName, actorNumber, transform.position),
			seenAmount, time));
	}
}
