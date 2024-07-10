using Splines;
using System;
using UnityEngine;

namespace CustomTimeline
{
    [Serializable]
    public class RouteSetting
    {
        public bool enable;
        public ConstraintType constraintType;
        public bool lockOpposite;
        public bool invFacing;
        public Vector3 facingRotation;
        public Vector3 extendingOffset;
        public SplineData route = new SplineData();
        public AnimationCurve moveCurve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));
        public AnimationCurve lookAtCurve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));
        public AnimationCurve lookAtWeightCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0), new Keyframe(1, 1, 0, 1));

        public Vector3 GetPositionOS(float rate)
        {
            return SplineUtil.GetPoint(route, SampleMoveFactor(rate));
        }

        public Vector3 GetDirectionOS(float rate)
        {
            return SplineUtil.GetDirection(route, SampleLookAtFactor(rate));
        }

        public float SampleMoveFactor(float rate)
        {
            return moveCurve.Evaluate(rate);
        }

        public float SampleLookAtFactor(float rate)
        {
            return lookAtCurve.Evaluate(rate);
        }

        public float SampleLookAtWeight(float rate)
        {
            return lookAtWeightCurve.Evaluate(rate);
        }

        public enum ConstraintType { None, Facing, Extending, FacingAndExtending }
    }
}