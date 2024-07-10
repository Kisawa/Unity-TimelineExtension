using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [System.Serializable]
    public class ProxyVector : ProxyBase<Vector4>
    {
        public override Vector4 Default => Vector4.zero;

#if UNITY_EDITOR
        [SerializeField] int channelCount = 4;
#endif

        protected override Vector4 Interpolation(float rate)
        {
            return origin + (end - origin) * rate;
        }

        public override Proxy Copy()
        {
            ProxyVector proxy = new ProxyVector();
            proxy.origin = origin;
            proxy.end = end;
            proxy.channelCount = channelCount;
            return proxy;
        }
    }
}