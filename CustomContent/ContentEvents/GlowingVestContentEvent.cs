namespace UnlistedEntities.CustomContent.ContentEvents;

public class GlowingVestContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_GlowingVest_0",
		"Content_GlowingVest_1",
		"Content_GlowingVest_2",
		"Content_GlowingVest_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 15f;

	protected override string DisplayName => "GlowingVest";
}
