using UnityEngine;

public class TranqProjectileLogic : MonoBehaviour
{
    private Projectile m_projectile;

    private void Awake()
    {
        m_projectile = GetComponent<Projectile>();
    }

    private void OnEnable()
    {
        if (m_projectile != null)
        {
            m_projectile.hitAction += OnHit;
        }
    }

    private void OnDisable()
    {
        if (m_projectile != null)
        {
            m_projectile.hitAction -= OnHit;
        }
    }

    private void OnHit(RaycastHit hit)
    {
        Player hitPlayer = hit.transform.GetComponentInParent<Player>();
        if (hitPlayer != null)
        {
            // Logic for hitting a player
            Debug.Log($"[TranqGun] Hit player: {hitPlayer.name}");

            if (hitPlayer.refs.view.IsMine)
            {
                var bot = hitPlayer.GetComponentInChildren<Bot>();
                if (bot != null)
                {
                    bot.targetPlayer = null;
                    for (int i = 0; i < PlayerHandler.instance.players.Count; i++)
                    {
                        bot.IgnoreTargetFor(PlayerHandler.instance.players[i], 10f);
                    }
                }
            }

            // Example: apply a custom effect or log the hit
            // hitPlayer.CallTakeDamageAndAddForceAndFallWithFallof(0f, Vector3.zero, 0.5f, hit.point, 1f);
        }
    }
}
