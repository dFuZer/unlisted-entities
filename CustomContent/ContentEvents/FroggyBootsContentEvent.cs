namespace UnlistedEntities.CustomContent.ContentEvents;

public class FroggyBootsContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_FroggyBoots_0",
		"Content_FroggyBoots_1",
		"Content_FroggyBoots_2",
		"Content_FroggyBoots_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 15f;

	protected override string DisplayName => "FroggyBoots";
}
