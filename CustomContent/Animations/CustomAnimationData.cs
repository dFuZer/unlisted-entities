
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimationKeyFrame
{
    public float time;
    public Quaternion rotationValue;
    public string bonePath;
}

public enum InterpolationType
{
    Lerp,
    Slerp
}

public class AnimationCallback
{
    public float time;
    public Action callback;
    public bool hasBeenCalled = false;
}

public class CustomAnimation
{
    public List<CustomAnimationKeyFrame> keyFrames;
    public Dictionary<string, List<CustomAnimationKeyFrame>> boneKeyFrames;
    public float duration;
    public List<AnimationCallback> animationCallbacks;
    public float speedMultiplier = 1.0f;
    public InterpolationType interpolationType;


    public CustomAnimation(List<CustomAnimationKeyFrame> keyFrames, InterpolationType interpolationType)
    {
        this.keyFrames = keyFrames;
        this.interpolationType = interpolationType;
        this.boneKeyFrames = new Dictionary<string, List<CustomAnimationKeyFrame>>();
        this.animationCallbacks = new List<AnimationCallback>();
        foreach (var keyFrame in keyFrames)
        {
            if (!boneKeyFrames.ContainsKey(keyFrame.bonePath))
            {
                boneKeyFrames[keyFrame.bonePath] = new List<CustomAnimationKeyFrame>();
            }
            boneKeyFrames[keyFrame.bonePath].Add(keyFrame);
        }
    }

    public void RegisterCallbackOnTime(float time, Action callback)
    {
        animationCallbacks.Add(new AnimationCallback { time = time, callback = callback });
    }

    // we'll use that later when we do interpolation
    public void AddInitialValueKeyframes(GameObject animationRig)
    {
        foreach (var bonePath in boneKeyFrames.Keys)
        {
            var keyframe = new CustomAnimationKeyFrame();
            keyframe.time = 0;
            keyframe.rotationValue = animationRig.transform.Find(bonePath).localRotation;
            keyframe.bonePath = bonePath;
            boneKeyFrames[bonePath].Add(keyframe);
        }
    }

    public void AddResetKeyframesAtEnd(GameObject animationRig, float animationEndTime)
    {
        foreach (var bonePath in boneKeyFrames.Keys)
        {
            var keyframe = new CustomAnimationKeyFrame();
            keyframe.time = animationEndTime;
            keyframe.rotationValue = animationRig.transform.Find(bonePath).localRotation;
            keyframe.bonePath = bonePath;
            boneKeyFrames[bonePath].Add(keyframe);
        }
    }

    public void Finalize()
    {
        SortKeyFrames();
        var maxTimeStamp = 0f;
        foreach (var bonePath in boneKeyFrames.Keys)
        {
            foreach (var keyFrame in boneKeyFrames[bonePath])
            {
                if (keyFrame.time > maxTimeStamp)
                {
                    maxTimeStamp = keyFrame.time;
                }
            }
        }
        duration = maxTimeStamp;
    }

    public void LogKeyFrames()
    {
        foreach (var bonePath in boneKeyFrames.Keys)
        {
            foreach (var keyFrame in boneKeyFrames[bonePath])
            {
                DbsContentApi.Modules.Logger.Log($"CustomAnimation: {keyFrame.time} {keyFrame.rotationValue} {keyFrame.bonePath}");
            }
        }
    }

    public void SortKeyFrames()
    {
        foreach (var bonePath in boneKeyFrames.Keys)
        {
            boneKeyFrames[bonePath].Sort((a, b) => a.time.CompareTo(b.time));
        }
    }

    public void SetValuesForFrame(GameObject animationRig, float timeSinceStart)
    {
        // Apply speed multiplier to the input time
        float adjustedTime = timeSinceStart * speedMultiplier;

        foreach (var callback in animationCallbacks)
        {
            if (adjustedTime >= callback.time && !callback.hasBeenCalled)
            {
                callback.callback.Invoke();
                callback.hasBeenCalled = true;
            }
        }

        foreach (var bonePath in boneKeyFrames.Keys)
        {
            var keyframes = boneKeyFrames[bonePath];
            if (keyframes.Count == 0) continue;

            // Use adjustedTime for finding keyframes
            CustomAnimationKeyFrame startKey = keyframes[0];
            CustomAnimationKeyFrame endKey = keyframes[keyframes.Count - 1];

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (adjustedTime >= keyframes[i].time && adjustedTime <= keyframes[i + 1].time)
                {
                    startKey = keyframes[i];
                    endKey = keyframes[i + 1];
                    break;
                }
            }

            Transform boneTransform = animationRig.transform.Find(bonePath);
            if (boneTransform == null) continue;

            if (startKey == endKey || adjustedTime >= endKey.time)
            {
                boneTransform.localRotation = endKey.rotationValue;
            }
            else
            {
                // Use adjustedTime for t calculation
                float range = endKey.time - startKey.time;
                float t = Mathf.Clamp01((adjustedTime - startKey.time) / range);

                boneTransform.localRotation = interpolationType == InterpolationType.Slerp
                    ? Quaternion.Slerp(startKey.rotationValue, endKey.rotationValue, t)
                    : Quaternion.Lerp(startKey.rotationValue, endKey.rotationValue, t);
            }
        }
    }
    private Quaternion GetInterpolatedRotation(Quaternion a, Quaternion b, float t)
    {
        return interpolationType switch
        {
            InterpolationType.Slerp => Quaternion.Slerp(a, b, t),
            InterpolationType.Lerp => Quaternion.Lerp(a, b, t),
            _ => Quaternion.Lerp(a, b, t)
        };
    }
}