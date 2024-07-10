using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    public abstract class Proxy
    {
        public abstract void Apply(float rate);
        public abstract void Reset();
        public abstract Proxy Copy();
    }
}