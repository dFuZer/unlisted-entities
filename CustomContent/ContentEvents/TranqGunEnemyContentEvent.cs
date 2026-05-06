namespace UnlistedEntities.CustomContent.ContentEvents;

public class TranqGunEnemyContentEvent : SimpleContentEvent
{
	public static readonly string[] COMMENTS =
	{
		"Content_TranqGunEnemy_0",
		"Content_TranqGunEnemy_1",
		"Content_TranqGunEnemy_2",
		"Content_TranqGunEnemy_3",
	};

	protected override string[] Comments => COMMENTS;

	protected override float Value => 50f;

	protected override string DisplayName => "TranqGunEnemy";
}
