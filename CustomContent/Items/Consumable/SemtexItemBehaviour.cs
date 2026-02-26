using System;
using UnityEngine;

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

        onStickSfx.Play(transform.position);
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