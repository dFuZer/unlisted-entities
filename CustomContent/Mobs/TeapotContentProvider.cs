using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent;

public class TeapotContentProvider : MonsterContentProvider
{
	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(GetContentEvent<TeapotContentEvent>(), seenAmount, time));
	}
}