namespace UnlistedEntities.CustomContent.ContentEvents;

public class CursedDollContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_CursedDoll_0",
		"Content_CursedDoll_1",
		"Content_CursedDoll_2",
		"Content_CursedDoll_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 15f;

	protected override string DisplayName => "CursedDoll";
}
