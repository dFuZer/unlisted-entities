namespace UnlistedEntities.CustomContent.ContentEvents;

public class SemtexStickMonsterContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_SemtexStickMonster_0",
		"Content_SemtexStickMonster_1",
		"Content_SemtexStickMonster_2",
		"Content_SemtexStickMonster_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 80f;

	protected override string DisplayName => "SemtexStickMonster";
}
