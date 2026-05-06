namespace UnlistedEntities.CustomContent.ContentEvents;

public class InvisiblePlayerContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_InvisiblePlayer_0",
		"Content_InvisiblePlayer_1",
		"Content_InvisiblePlayer_2",
		"Content_InvisiblePlayer_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 50f;

	protected override string DisplayName => "InvisiblePlayer";
}
