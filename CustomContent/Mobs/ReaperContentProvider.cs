using System.Collections.Generic;
using UnityEngine;

namespace UnlistedEntities.CustomContent;

public class ReaperContentProvider : MonsterContentProvider
{
	public override void GetContent(List<ContentEventFrame> contentEvents, float seenAmount, Camera camera, float time)
	{
		contentEvents.Add(new ContentEventFrame(GetContentEvent<ReaperContentEvent>(), seenAmount, time));
	}
}
