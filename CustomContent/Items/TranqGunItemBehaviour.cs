using Photon.Pun;
using UnityEngine;
using Zorro.Core.Serizalization;
using DbsContentApi;

public class TranqGunItemBehaviour : ItemInstanceBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float maxCharge = 100f;
    public int maxCharges = 10; // Fewer charges for a tranq gun

    private BatteryEntry m_batteryEntry;
    private Player playerHoldingItem;
    private float sinceFire;

    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        playerHoldingItem = base.transform.root.GetComponent<Player>();
        if (!data.TryGetEntry<BatteryEntry>(out m_batteryEntry))
        {
            m_batteryEntry = new BatteryEntry
            {
                m_charge = maxCharge,
                m_maxCharge = maxCharge
            };
            data.AddDataEntry(m_batteryEntry);
        }
        itemInstance.RegisterRPC(ItemRPC.RPC1, RPCA_FireTranq);
    }

    private void FixedUpdate()
    {
        sinceFire += Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (isHeldByMe && m_batteryEntry.m_charge > 0f && Player.localPlayer.input.clickIsPressed && !Player.localPlayer.HasLockedInput() && sinceFire > 0.5f)
        {
            Fire();
        }
    }

    private void Fire()
    {
        m_batteryEntry.m_charge -= m_batteryEntry.m_maxCharge / (float)maxCharges;
        m_batteryEntry.SetDirty();
        sinceFire = 0f;

        BinarySerializer binarySerializer = new BinarySerializer();
        binarySerializer.WriteFloat3(firePoint.position);
        binarySerializer.WriteFloat3(firePoint.forward);
        itemInstance.CallRPC(ItemRPC.RPC1, binarySerializer);
    }

    public void RPCA_FireTranq(BinaryDeserializer deserializer)
    {
        if (isHeld)
        {
            Vector3 position = deserializer.ReadFloat3();
            Vector3 forward = deserializer.ReadFloat3();
            sinceFire = 0f;

            GameObject obj = Object.Instantiate(projectilePrefab, position, Quaternion.LookRotation(forward));

            GameAPI.instance.objectSpawnedAction(obj);

            // Visual/Audio feedback
            GamefeelHandler.instance.perlin.AddShake(base.transform.position, 1.5f, 0.1f, 10f, 30f);
            if (playerHoldingItem != null && playerHoldingItem.refs.ragdoll != null)
            {
                var bodyPart = playerHoldingItem.refs.ragdoll.GetBodypart(BodypartType.Item);
                if (bodyPart != null && bodyPart.rig != null)
                {
                    bodyPart.rig.AddForce(base.transform.forward * -0.5f, ForceMode.VelocityChange);
                }
            }
        }
    }
}
