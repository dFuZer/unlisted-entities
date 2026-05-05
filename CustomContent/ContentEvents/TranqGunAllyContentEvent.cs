using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class TranqGunAllyContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_TranqGunAlly_0",
		"Content_TranqGunAlly_1",
		"Content_TranqGunAlly_2",
		"Content_TranqGunAlly_3",
	};

	public TranqGunAllyContentEvent() { }

	public TranqGunAllyContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 20f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "TranqGunAlly";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
