namespace UnlistedEntities.CustomContent.ContentEvents;

public class ElectricGrenadeAllyContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_ElectricGrenadeAlly_0",
		"Content_ElectricGrenadeAlly_1",
		"Content_ElectricGrenadeAlly_2",
		"Content_ElectricGrenadeAlly_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 45f;

	protected override string DisplayName => "ElectricGrenadeAlly";
}
