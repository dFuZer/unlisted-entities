using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to a short-lived trigger object at the hit location.
/// </summary>
public class BatHitAllyContentProvider : ContentProvider
{
	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(new BatHitAllyContentEvent(), seenAmount, time));
	}
}
