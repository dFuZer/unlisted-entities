using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent.ContentEvents;

/// <summary>
/// Attach to a short-lived trigger object at the explosion location.
/// </summary>
public class ElectricGrenadeMonsterContentProvider : ContentProvider
{
	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(new ElectricGrenadeMonsterContentEvent(), seenAmount, time));
	}
}
