using Zorro.Core;

namespace UnlistedEntities.CustomContent.ContentEvents;

public class TranqGunEnemyContentEvent : MonsterContentEvent
{
	public static string[] COMMENTS = new string[]
	{
		"Content_TranqGunEnemy_0",
		"Content_TranqGunEnemy_1",
		"Content_TranqGunEnemy_2",
		"Content_TranqGunEnemy_3",
	};

	public override float GetContentValue() => 50f;

	public override ushort GetID() => DbsContentApi.Modules.ContentEvents.GetEventID(GetType().Name);

	public override string GetName() => "TranqGunEnemy";

	public override string[] GetAllComments() => COMMENTS;

	public override Comment GenerateComment() => new Comment(COMMENTS.GetRandom());
}
