using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [System.Serializable]
    public class ProxyColor : ProxyBase<Color>
    {
        public override Color Default => Color.white;

#if UNITY_EDITOR
        [SerializeField] bool HDR;
#endif

        protected override Color Interpolation(float rate)
        {
            return Color.LerpUnclamped(origin, end, rate);
        }

        public override Proxy Copy()
        {
            ProxyColor proxy = new ProxyColor();
            proxy.origin = origin;
            proxy.end = end;
            proxy.HDR = HDR;
            return proxy;
        }
    }
}