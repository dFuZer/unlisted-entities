namespace UnlistedEntities.CustomContent.ContentEvents;

public class AngelWingsContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_AngelWings_0",
		"Content_AngelWings_1",
		"Content_AngelWings_2",
		"Content_AngelWings_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 15f;

	protected override string DisplayName => "AngelWings";
}
