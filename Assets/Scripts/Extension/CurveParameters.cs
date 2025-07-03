using UnityEngine;

[System.Serializable]
public class CurveParameters
{
    [Header("Positioning")]
    public AnimationCurve positioning = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
    public float positioningInfluence = 1f;

    [Header("Rotation")]
    public AnimationCurve rotation = new AnimationCurve(new Keyframe(0, -30), new Keyframe(0.5f, 0), new Keyframe(1, 30));
    public float rotationInfluence = 1f;
}