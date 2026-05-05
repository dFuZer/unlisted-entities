using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class InvisiblePlayerContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_InvisiblePlayer_0",
		"Content_InvisiblePlayer_1",
		"Content_InvisiblePlayer_2",
		"Content_InvisiblePlayer_3",
	};

	public InvisiblePlayerContentEvent() { }

	public InvisiblePlayerContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 50f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "InvisiblePlayer";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
