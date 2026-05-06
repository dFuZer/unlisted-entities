using UnlistedEntities.CustomContent.ContentEvents;

public class PopitItemBehaviour : ThrowableExplosiveBehaviour
{
	protected override void OnExplode()
	{
		TrySpawnExplosionContentProvider<PopitExplosionContentProvider>();
	}
}
