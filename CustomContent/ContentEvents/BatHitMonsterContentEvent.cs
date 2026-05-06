namespace UnlistedEntities.CustomContent.ContentEvents;

public class BatHitMonsterContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_BatHitMonster_0",
		"Content_BatHitMonster_1",
		"Content_BatHitMonster_2",
		"Content_BatHitMonster_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 35f;

	protected override string DisplayName => "BatHitMonster";
}
