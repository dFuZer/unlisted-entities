namespace UnlistedEntities.CustomContent.ContentEvents;

public class SilverFulminateMonsterContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_SilverFulminateMonster_0",
		"Content_SilverFulminateMonster_1",
		"Content_SilverFulminateMonster_2",
		"Content_SilverFulminateMonster_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 110f;

	protected override string DisplayName => "SilverFulminateMonster";
}
