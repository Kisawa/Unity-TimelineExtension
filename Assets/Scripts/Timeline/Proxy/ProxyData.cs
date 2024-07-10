using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [System.Serializable]
    public class ProxyData
    {
        public string name;
        [SerializeReference]
        public Proxy proxy = new ProxyVector();
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));

        public void Apply(float rate)
        {
            rate = curve.Evaluate(rate);
            proxy.Apply(rate);
        }

        public void Reset()
        {
            if (proxy == null)
                return;
            proxy.Reset();
        }
    }
}