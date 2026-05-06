namespace UnlistedEntities.CustomContent.ContentEvents;

public class SemtexStickAllyContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_SemtexStickAlly_0",
		"Content_SemtexStickAlly_1",
		"Content_SemtexStickAlly_2",
		"Content_SemtexStickAlly_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 30f;

	protected override string DisplayName => "SemtexStickAlly";
}
