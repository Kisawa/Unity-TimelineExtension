using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    public abstract class ProxyBase<T> : Proxy where T : struct
    {
        public abstract T Default { get; }
        public T origin;
        public T end;

        T val;
        public T Val => val;

        public ProxyBase()
        {
            origin = Default;
            end = Default;
            val = origin;
        }

        public override void Apply(float rate)
        {
            val = Interpolation(rate);
        }

        public override void Reset()
        {
            val = origin;
        }

        protected abstract T Interpolation(float rate);
    }
}