using UnlistedEntities.CustomContent.ContentEvents;

public class SilverFulminateItemBehaviour : ThrowableExplosiveBehaviour
{
	protected override void OnExplode()
	{
		TrySpawnExplosionContentProvider<SilverFulminateExplosionContentProvider>();
	}
}
