using UnlistedEntities.CustomContent.ContentEvents;

public class GrenadeItemBehaviour : ThrowableExplosiveBehaviour
{
	protected override void OnExplode()
	{
		TrySpawnExplosionContentProvider<GrenadeExplosionContentProvider>();
	}
}
