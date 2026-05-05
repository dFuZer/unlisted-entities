using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class SemtexStickMonsterContentEvent : MonsterContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_SemtexStickMonster_0",
		"Content_SemtexStickMonster_1",
		"Content_SemtexStickMonster_2",
		"Content_SemtexStickMonster_3",
	};

	public override float GetContentValue() => 80f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "SemtexStickMonster";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());
}
