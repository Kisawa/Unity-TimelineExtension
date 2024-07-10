using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [System.Serializable]
    public class ProxyInt : ProxyBase<int>
    {
        public override int Default => 0;

        protected override int Interpolation(float rate)
        {
            return Mathf.RoundToInt(origin + (end - origin) * rate);
        }

        public override Proxy Copy()
        {
            ProxyInt proxy = new ProxyInt();
            proxy.origin = origin;
            proxy.end = end;
            return proxy;
        }
    }
}