using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [System.Serializable]
    public class ProxyFloat : ProxyBase<float>
    {
        public override float Default => 0;

        protected override float Interpolation(float rate)
        {
            return origin + (end - origin) * rate;
        }

        public override Proxy Copy()
        {
            ProxyFloat proxy = new ProxyFloat();
            proxy.origin = origin;
            proxy.end = end;
            return proxy;
        }
    }
}