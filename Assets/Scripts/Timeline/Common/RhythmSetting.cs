using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [Serializable]
    public class RhythmSetting
    {
        public bool Enable;
        public float RhythmStart;
        public float RhythmEnd;
        public AnimationCurve RhythmCurve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));

        public float CalcRhythmTime(float time)
        {
            if (!Enable)
                return time;
            float t = Mathf.Clamp(time, RhythmStart, RhythmEnd);
            if (t != time)
                return time;
            t -= RhythmStart;
            float span = RhythmEnd - RhythmStart;
            float rate = t / span;
            return span * RhythmCurve.Evaluate(rate) + RhythmStart;
        }
    }
}