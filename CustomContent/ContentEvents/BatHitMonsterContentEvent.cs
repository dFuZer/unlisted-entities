using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class BatHitMonsterContentEvent : MonsterContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_BatHitMonster_0",
		"Content_BatHitMonster_1",
		"Content_BatHitMonster_2",
		"Content_BatHitMonster_3",
	};

	public override float GetContentValue() => 35f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "BatHitMonster";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());
}
