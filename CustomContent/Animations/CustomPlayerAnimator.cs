using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
public class CustomPlayerAnimator : MonoBehaviour
{
    GameObject animationRigRoot;
    private CustomAnimation currentCustomAnimation;
    private float animationStartTime = 0f;
    public void Awake()
    {
        // rigroot is the gameobject this component is attached to
        animationRigRoot = gameObject;
    }

    public void TryActivateThrowAnimation(Action callbackOnThrowFrame)
    {
        if (currentCustomAnimation != null) return;
        var frames = new List<CustomAnimationKeyFrame>();

        string torso = "Rig/Armature/Hip/Torso";
        string armR = "Rig/Armature/Hip/Torso/Arm_R";
        string elbowR = "Rig/Armature/Hip/Torso/Arm_R/Elbow_R";
        // Arm_L and Hand_R were not explicitly defined with unique curves in the provided YAML, 
        // so I kept your timing or used the closest available data.

        // TORSO - Keyframes at 0, 11, 12, 13, 16, 19, 20
        // --- TORSO ---
        // First movement starts at 0.33s (Frame 10)
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 10f,
            bonePath = torso,
            rotationValue = Quaternion.Euler(0, 70, 0)
        });
        // Full rotation reached at 0.66s (Frame 20)
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 23f,
            bonePath = torso,
            rotationValue = Quaternion.Euler(0, -70, 0)
        });

        // --- ARM_R ---
        // Prep position at 0.3s (Frame 9)
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 5f,
            bonePath = armR,
            rotationValue = new Quaternion(0.07831256f, 0.13297257f, -0.9330111f, 0.32507813f)
        });
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 17f,
            bonePath = armR,
            rotationValue = Quaternion.Euler(-14f, 64f, -32f)
        });
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 25f,
            bonePath = armR,
            rotationValue = new Quaternion(0.4599232f, 0.19515157f, -0.13305375f, 0.8559692f)
        });

        // --- ELBOW_R ---
        // Static until 0.36s (Frame 11)
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 11f,
            bonePath = elbowR,
            rotationValue = new Quaternion(0.40379998f, -0.48517668f, 0.3436905f, 0.6952884f)
        });

        frames.Add(new CustomAnimationKeyFrame
        {
            time = 17f,
            bonePath = elbowR,
            rotationValue = Quaternion.Euler(36f, -42f, 22f)
        });
        // Fully extended at 0.66s (Frame 20)
        frames.Add(new CustomAnimationKeyFrame
        {
            time = 24f,
            bonePath = elbowR,
            rotationValue = new Quaternion(0.09231234f, -0.3148702f, 0.22914469f, 0.91642123f)
        });

        currentCustomAnimation = new CustomAnimation(frames, InterpolationType.Lerp);
        currentCustomAnimation.RegisterCallbackOnTime(25f, callbackOnThrowFrame);
        currentCustomAnimation.LogKeyFrames();
        currentCustomAnimation.speedMultiplier = 35f;

        currentCustomAnimation.AddInitialValueKeyframes(animationRigRoot);
        currentCustomAnimation.AddResetKeyframesAtEnd(animationRigRoot, 28f);
        currentCustomAnimation.Finalize();
        animationStartTime = Time.time;
        DbsContentApi.Modules.Logger.Log($"CustomPlayerAnimator: Activated throw animation");
        currentCustomAnimation.LogKeyFrames();

        // Sync animation to other clients so they see this player's throw
        var pv = GetComponentInParent<PhotonView>();
        if (pv != null && pv.IsMine)
            pv.RPC(nameof(PlayerRPCBridge.RPCA_PlayThrowAnimation), RpcTarget.Others);
    }

    // set lateUpdate to modify the rig
    void LateUpdate()
    {
        if (currentCustomAnimation != null)
        {
            var timeSinceStart = Time.time - animationStartTime;

            // Adjust the duration check to account for speed
            float effectiveDuration = currentCustomAnimation.duration / currentCustomAnimation.speedMultiplier;

            if (timeSinceStart > effectiveDuration)
            {
                DbsContentApi.Modules.Logger.Log($"CustomAnimation finished");
                currentCustomAnimation = null;
                return;
            }
            else
            {
                // SetValuesForFrame handles the speedMultiplier internally
                currentCustomAnimation.SetValuesForFrame(animationRigRoot, timeSinceStart);
            }
        }
    }
}


[HarmonyPatch]
public class CustomPlayerAnimatorPatches
{

    [HarmonyPatch(typeof(Player), "Start")]
    [HarmonyPostfix]
    static void StartPostfixPatch(Player __instance, ref IEnumerator __result)
    {
        if (__instance.ai) return;

        __result = WrapEnumerator(__instance, __result);
    }

    static IEnumerator WrapEnumerator(Player __instance, IEnumerator original)
    {
        while (original.MoveNext())
        {
            yield return original.Current;
        }

        var animationRigRoot = __instance.transform.Find("AnimationRig").gameObject;
        if (animationRigRoot.GetComponent<CustomPlayerAnimator>() == null)
        {
            animationRigRoot.AddComponent<CustomPlayerAnimator>();
        }
    }
}