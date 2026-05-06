using System;
using UnityEngine;
using DbsContentApi.Modules.Utility;
using UnlistedEntities.CustomContent;
using UnlistedEntities.CustomContent.ContentEvents;

public class SemtexItemBehaviour : ThrowableExplosiveBehaviour
{
    private Vector3 relativePos;
    private Quaternion relativeRot;
    private Transform? hitTransform;
    private Rigidbody? rb;
    public SFX_Instance onStickSfx;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only stick if we are not held and we haven't stuck to anything yet
        if (isHeld || hitTransform != null) return;

        // Requirement: Cannot stick to another item
        if (collision.gameObject.GetComponentInParent<ItemInstance>() != null) return;

        // Stick to anything else (Scenery, Players, etc.)
        Stick(collision);
    }

    private void Stick(Collision collision)
    {
        hitTransform = collision.transform;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Store relative position and rotation to prevent snapping
        relativePos = hitTransform.InverseTransformPoint(transform.position);
        relativeRot = Quaternion.Inverse(hitTransform.rotation) * transform.rotation;

        if (onStickSfx != null)
            onStickSfx.Play(transform.position);

        if (!isSimulatedByMe)
            return;

        Player hitPlayer = hitTransform.GetComponentInParent<Player>();
        if (hitPlayer == null)
            return;

        if (CustomItems.TemporaryContentTriggerPrefab == null)
        {
            DbsContentApi.Modules.Logger.LogError("SemtexItemBehaviour.Stick: TemporaryContentTriggerPrefab is null; cannot spawn Semtex stick content provider.");
            return;
        }

        GameObject trigger = ObjectHelper.CreateTemporaryTriggerObject(50, CustomItems.TemporaryContentTriggerPrefab);
        if (trigger == null)
        {
            DbsContentApi.Modules.Logger.LogError("SemtexItemBehaviour.Stick: CreateTemporaryTriggerObject returned null.");
            return;
        }

        trigger.transform.position = transform.position;
        if (hitPlayer.ai)
        {
            SemtexStickMonsterContentProvider provider = trigger.AddComponent<SemtexStickMonsterContentProvider>();
        }
        else if (hitPlayer.refs.view?.Owner != null)
        {
            SemtexStickAllyContentProvider provider = trigger.AddComponent<SemtexStickAllyContentProvider>();
        }
        else
        {
            DbsContentApi.Modules.Logger.LogError("SemtexItemBehaviour.Stick: stuck to human Player but PhotonView.Owner is missing; ally stick content provider not configured.");
            UnityEngine.Object.Destroy(trigger);
        }
    }

    protected override void OnExplode()
    {
        TrySpawnExplosionContentProvider<SemtexExplosionContentProvider>();
    }

    protected override void Update()
    {
        base.Update();

        if (hitTransform != null)
        {
            // If the object we stuck to is destroyed, we should probably just fall or explode
            if (hitTransform == null)
            {
                hitTransform = null;
                if (rb != null) rb.isKinematic = false;
                return;
            }

            transform.position = hitTransform.TransformPoint(relativePos);
            transform.rotation = hitTransform.rotation * relativeRot;
        }
    }
}