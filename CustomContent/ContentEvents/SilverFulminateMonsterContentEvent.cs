using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class SilverFulminateMonsterContentEvent : MonsterContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_SilverFulminateMonster_0",
		"Content_SilverFulminateMonster_1",
		"Content_SilverFulminateMonster_2",
		"Content_SilverFulminateMonster_3",
	};

	public override float GetContentValue() => 110f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "SilverFulminateMonster";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());
}
