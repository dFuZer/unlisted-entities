namespace UnlistedEntities.CustomContent.ContentEvents;

public class BatHitAllyContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_BatHitAlly_0",
		"Content_BatHitAlly_1",
		"Content_BatHitAlly_2",
		"Content_BatHitAlly_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 20f;

	protected override string DisplayName => "BatHitAlly";
}
