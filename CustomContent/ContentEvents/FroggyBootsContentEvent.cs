using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class FroggyBootsContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_FroggyBoots_0",
		"Content_FroggyBoots_1",
		"Content_FroggyBoots_2",
		"Content_FroggyBoots_3",
	};

	public FroggyBootsContentEvent() { }

	public FroggyBootsContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 15f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "FroggyBoots";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
