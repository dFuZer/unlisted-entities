using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class ElectricGrenadeMonsterContentEvent : MonsterContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_ElectricGrenadeMonster_0",
		"Content_ElectricGrenadeMonster_1",
		"Content_ElectricGrenadeMonster_2",
		"Content_ElectricGrenadeMonster_3",
	};

	public override float GetContentValue() => 50f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "ElectricGrenadeMonster";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());
}
