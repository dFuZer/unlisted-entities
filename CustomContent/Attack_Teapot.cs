using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Teapot-specific attack behavior.
/// Fires water projectiles at players when in range.
/// </summary>
public class Attack_Teapot : MonoBehaviour
{
    public Transform beakTransform = null!;
    public GameObject waterProjectilePrefab = null!;

    public float attackRange = 15f;
    public float attackDuration = 3f;
    public float fireInterval = 0.1f;
    public float projectileSpreadAngle = 4f;

    public SFX_Instance ambiantBoilingWaterSfx = null!;
    public SFX_Instance hardBoilingWaterSfx = null!;

    private Bot _bot = null!;
    private PhotonView _view = null!;
    private Player _player = null!;

    private bool _isAttacking;
    private Coroutine? _soundLoopRoutine;
    private SFX_Instance? _currentlyPlayingSfx;

    private void Start()
    {
        _bot = GetComponent<Bot>();
        _view = GetComponent<PhotonView>();
        _player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        SFX_Instance desiredSfx = _isAttacking ? hardBoilingWaterSfx : ambiantBoilingWaterSfx;

        if (_currentlyPlayingSfx != desiredSfx)
        {
            if (_soundLoopRoutine != null)
            {
                StopCoroutine(_soundLoopRoutine);
                _soundLoopRoutine = null;
            }

            _currentlyPlayingSfx = desiredSfx;
            if (desiredSfx != null)
            {
                _soundLoopRoutine = StartCoroutine(LoopSoundCoroutine(desiredSfx));
            }
        }

        if (_bot == null || _view == null || !_view.IsMine || _isAttacking || _bot.targetPlayer == null)
            return;

        var target = _bot.targetPlayer;
        float distance = Vector3.Distance(transform.position, target.Center());

        if (distance <= attackRange && _bot.AbleToAttack(attackRange, 0.5f, _player))
        {
            StartCoroutine(AttackBurstCoroutine(target));
        }
    }

    /// <summary>
    /// Executes a burst of water projectile attacks.
    /// </summary>
    /// <param name="targetPlayer">The player to target.</param>
    private IEnumerator AttackBurstCoroutine(Player targetPlayer)
    {
        _isAttacking = true;
        _bot.busy = true;
        _bot.attacking = true;

        float elapsed = 0f;
        float fireTimer = 0f;

        while (elapsed < attackDuration)
        {
            if (targetPlayer == null || _player == null || _player.NoControl()) break;

            if (_view.IsMine) _bot.LookAt(targetPlayer.Center(), 10f);

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                bool canSeeTarget = _bot.CanSeeTarget(targetPlayer.Center(), 10f);
                if(canSeeTarget)
                {
                    FireProjectileAtTarget(targetPlayer);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _bot.busy = false;
        _bot.attacking = false;
        _isAttacking = false;
    }

    /// <summary>
    /// Calculates and network-spawns a water projectile toward the target.
    /// </summary>
    /// <param name="targetPlayer">The player to fire at.</param>
    private void FireProjectileAtTarget(Player targetPlayer)
    {
        if (beakTransform == null || waterProjectilePrefab == null) return;

        Vector3 origin = beakTransform.position;
        Vector3 baseDir = (targetPlayer.Center() - origin).normalized;
        Quaternion spreadRot = Quaternion.Euler(
            Random.Range(-projectileSpreadAngle, projectileSpreadAngle),
            Random.Range(-projectileSpreadAngle, projectileSpreadAngle),
            0f
        );

        _view.RPC(nameof(RPCA_FireWaterProjectile), RpcTarget.All, origin, spreadRot * baseDir);
    }

    /// <summary>
    /// RPC handler to instantiate the projectile on all clients.
    /// </summary>
    [PunRPC]
    private void RPCA_FireWaterProjectile(Vector3 origin, Vector3 direction)
    {
        if (waterProjectilePrefab != null)
        {
            Instantiate(waterProjectilePrefab, origin, Quaternion.LookRotation(direction));
        }
    }

    /// <summary>
    /// Continuously loops an SFX instance.
    /// </summary>
    private IEnumerator LoopSoundCoroutine(SFX_Instance sfx)
    {
        while (_currentlyPlayingSfx == sfx)
        {
            sfx.Play(transform.position, local: false, volumeMultiplier: 1f);
            AudioClip clip = sfx.GetClip();
            yield return new WaitForSeconds(clip != null ? clip.length * 0.95f : 1f);
        }
    }

    private void OnDestroy()
    {
        if (_soundLoopRoutine != null) StopCoroutine(_soundLoopRoutine);
    }
}

