using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class GlowingVestContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_GlowingVest_0",
		"Content_GlowingVest_1",
		"Content_GlowingVest_2",
		"Content_GlowingVest_3",
	};

	public GlowingVestContentEvent() { }

	public GlowingVestContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 15f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "GlowingVest";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
