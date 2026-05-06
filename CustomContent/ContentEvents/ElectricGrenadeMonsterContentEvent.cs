namespace UnlistedEntities.CustomContent.ContentEvents;

public class ElectricGrenadeMonsterContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_ElectricGrenadeMonster_0",
		"Content_ElectricGrenadeMonster_1",
		"Content_ElectricGrenadeMonster_2",
		"Content_ElectricGrenadeMonster_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 50f;

	protected override string DisplayName => "ElectricGrenadeMonster";
}
