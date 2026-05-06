namespace UnlistedEntities.CustomContent.ContentEvents;

public class TranqGunAllyContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_TranqGunAlly_0",
		"Content_TranqGunAlly_1",
		"Content_TranqGunAlly_2",
		"Content_TranqGunAlly_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 20f;

	protected override string DisplayName => "TranqGunAlly";
}
