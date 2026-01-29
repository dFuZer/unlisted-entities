using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Modules.Logger.Patches
{
    public class BatBehaviour : ItemInstanceBehaviour
    {
        private Player player;
        private bool isSwinging = false;
        private bool isInitialized = false;

        // Swing parameters - GENTLE to prevent self-knockdown
        public float swingForce = 3f;
        public float swingDuration = 0.6f;
        public float cooldownTime = 0.8f;

        // Collision tracking
        private HashSet<Player> hitPlayers = new HashSet<Player>();

        public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
        {
            DbsContentApi.Modules.Logger.Log("ConfigItem called for BatBehaviour");

            player = GetComponentInParent<Player>();

            if (player != null)
            {
                DbsContentApi.Modules.Logger.Log("Player found!");
                DbsContentApi.Modules.Logger.Log($"Bat collider: {GetComponentInChildren<Collider>(true) != null}");
            }
            else
            {
                DbsContentApi.Modules.Logger.LogWarning("Could not find Player component");
            }

            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized || player == null) return;

            if (!this.isHeldByMe) return;
            if (Player.localPlayer != null && Player.localPlayer.HasLockedInput()) return;

            if (Player.localPlayer.input.clickWasPressed && !isSwinging)
            {
                DbsContentApi.Modules.Logger.Log("Click detected! Triggering swing...");
                TriggerSwing();
            }
        }

        void TriggerSwing()
        {
            hitPlayers.Clear();
            PerformSwingLocal();
        }

        private void PerformSwingLocal()
        {
            if (player == null || player.refs?.ragdoll == null)
            {
                DbsContentApi.Modules.Logger.LogError("Player or ragdoll missing!");
                return;
            }

            isSwinging = true;

            Bodypart handR = player.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
            Bodypart elbowR = player.refs.ragdoll.GetBodypart(BodypartType.Elbow_R);
            Bodypart armR = player.refs.ragdoll.GetBodypart(BodypartType.Arm_R);

            if (handR != null && elbowR != null && armR != null)
            {
                DbsContentApi.Modules.Logger.Log("Starting swing with manual hit detection!");
                StartCoroutine(PerformSwing(handR, elbowR, armR));
            }
            else
            {
                isSwinging = false;
            }
        }

        private IEnumerator PerformSwing(Bodypart hand, Bodypart elbow, Bodypart arm)
        {
            Vector3 lookDirection = player.refs.headPos.forward;
            Vector3 forceDirection = player.refs.headPos.forward;

            // Add strong downward component for baseball-style swing
            forceDirection += Vector3.down * 0.95f;  // Much more downward
            forceDirection = forceDirection.normalized;

            Vector3 swingDirection = (lookDirection + player.refs.headPos.right * 0.01f).normalized;

            Collider batCollider = GetComponentInChildren<Collider>(true);
            if (batCollider == null)
            {
                DbsContentApi.Modules.Logger.LogError("No bat collider found!");
                isSwinging = false;
                yield break;
            }

            float elapsed = 0f;

            // SWING LOOP WITH MANUAL HIT DETECTION
            while (elapsed < swingDuration)
            {
                // Gentle arm swing forces
                float progress = elapsed / swingDuration;
                float forceCurve = Mathf.Sin(progress * Mathf.PI);
                float currentForce = swingForce * forceCurve;

                hand.rig.AddForce(forceDirection * currentForce, ForceMode.VelocityChange);
                elbow.rig.AddForce(forceDirection * currentForce * 0.6f, ForceMode.VelocityChange);
                arm.rig.AddForce(forceDirection * currentForce * 0.4f, ForceMode.VelocityChange);

                // MANUAL HIT DETECTION using bat collider bounds
                Collider[] hits = Physics.OverlapBox(
                    batCollider.bounds.center,
                    batCollider.bounds.extents,
                    batCollider.transform.rotation,
                    -1,
                    QueryTriggerInteraction.Collide
                );

                foreach (Collider hit in hits)
                {
                    ProcessHit(hit, lookDirection);
                }

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            DbsContentApi.Modules.Logger.Log("Swing complete!");

            // Cooldown
            yield return new WaitForSeconds(cooldownTime - swingDuration);
            isSwinging = false;
            DbsContentApi.Modules.Logger.Log("Ready for next swing");
        }

        private void ProcessHit(Collider other, Vector3 forceDirection)
        {
            if (!isSwinging) return;

            DbsContentApi.Modules.Logger.Log($"Bat hit: {other.name} (root: {other.transform.root.name})");

            // Ignore self
            if (other.transform.root == player?.transform.root)
            {
                DbsContentApi.Modules.Logger.Log("Self-hit ignored");
                return;
            }

            // Find target
            Player hitPlayer = other.GetComponentInParent<Player>();
            if (hitPlayer != null && hitPlayer != Player.localPlayer && !hitPlayers.Contains(hitPlayer))
            {
                hitPlayers.Add(hitPlayer);
                DbsContentApi.Modules.Logger.Log($"*** HIT TARGET: {hitPlayer.name} ***");
                OnHitTarget(hitPlayer, forceDirection);
            }
        }

        private void OnHitTarget(Player hitPlayer, Vector3 forceDirection)
        {
            DbsContentApi.Modules.Logger.Log($"Tasing {hitPlayer.name}");

            if (hitPlayer.refs?.ragdoll != null)
            {
                hitPlayer.refs.ragdoll.TaseShock(5f);
                hitPlayer.refs.ragdoll.AddForce(forceDirection * 20f, ForceMode.VelocityChange);
                DbsContentApi.Modules.Logger.Log("Tase applied successfully!");
            }

            // Remove the bat from inventory
            GlobalPlayerData globalPlayerData;
            if (GlobalPlayerData.TryGetPlayerData(player.refs.view.Owner, out globalPlayerData))
            {
                PlayerInventory playerInventory = globalPlayerData.inventory;
                if (playerInventory != null && player.data.selectedItemSlot >= 0)
                {
                    DbsContentApi.Modules.Logger.Log($"Clearing inventory slot {player.data.selectedItemSlot}");

                    // Get the slot and clear it properly
                    InventorySlot slot;
                    if (playerInventory.TryGetSlot(player.data.selectedItemSlot, out slot))
                    {
                        slot.Clear(); // This syncs across network via RPC
                        DbsContentApi.Modules.Logger.Log("Bat removed from inventory slot");
                    }
                }
            }

            DbsContentApi.Modules.Logger.Log("Destroying bat after hit");
            Destroy(this.gameObject);
        }
    }
}