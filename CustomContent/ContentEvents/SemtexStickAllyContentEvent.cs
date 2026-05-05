using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class SemtexStickAllyContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_SemtexStickAlly_0",
		"Content_SemtexStickAlly_1",
		"Content_SemtexStickAlly_2",
		"Content_SemtexStickAlly_3",
	};

	public SemtexStickAllyContentEvent() { }

	public SemtexStickAllyContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 30f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "SemtexStickAlly";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
