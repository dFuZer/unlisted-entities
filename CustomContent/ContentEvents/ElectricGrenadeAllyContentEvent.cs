using System.Collections.Generic;
using UnityEngine;
using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class ElectricGrenadeAllyContentEvent : PlayerBaseEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_ElectricGrenadeAlly_0",
		"Content_ElectricGrenadeAlly_1",
		"Content_ElectricGrenadeAlly_2",
		"Content_ElectricGrenadeAlly_3",
	};

	public ElectricGrenadeAllyContentEvent() { }

	public ElectricGrenadeAllyContentEvent(string playerName, int actorNumber, Vector3 worldPosition)
		: base(playerName, actorNumber, worldPosition) { }

	public override float GetContentValue() => 45f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "ElectricGrenadeAlly";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment()
	{
		List<string> list = new List<string>(COMMENTS);
		return new Comment(list.GetRandom(), playerName);
	}
}
