using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    public partial class ProxyMixerBehaviour
    {
        Dictionary<string, Cache<ProxyFloat>> proxyFloat = new Dictionary<string, Cache<ProxyFloat>>();
        Dictionary<string, Cache<ProxyVector>> proxyVector = new Dictionary<string, Cache<ProxyVector>>();
        Dictionary<string, Cache<ProxyColor>> proxyColor = new Dictionary<string, Cache<ProxyColor>>();
        Dictionary<string, Cache<ProxyInt>> proxyInt = new Dictionary<string, Cache<ProxyInt>>();

        CacheBase GetCache(ProxyData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.name))
                return null;
            Type proxyType = data.proxy.GetType();
            if (proxyType == typeof(ProxyFloat))
                return GetFloatCache(data.name);
            if (proxyType == typeof(ProxyVector))
                return GetVectorCache(data.name);
            if (proxyType == typeof(ProxyColor))
                return GetColorCache(data.name);
            if (proxyType == typeof(ProxyInt))
                return GetIntCache(data.name);
            return null;
        }

        Cache<ProxyFloat> GetFloatCache(string name)
        {
            if (proxyFloat.TryGetValue(name, out Cache<ProxyFloat> cache))
                return cache;
            cache = new Cache<ProxyFloat>(name);
            proxyFloat.Add(name, cache);
            return cache;
        }

        Cache<ProxyVector> GetVectorCache(string name)
        {
            if (proxyVector.TryGetValue(name, out Cache<ProxyVector> cache))
                return cache;
            cache = new Cache<ProxyVector>(name);
            proxyVector.Add(name, cache);
            return cache;
        }

        Cache<ProxyColor> GetColorCache(string name)
        {
            if (proxyColor.TryGetValue(name, out Cache<ProxyColor> cache))
                return cache;
            cache = new Cache<ProxyColor>(name);
            proxyColor.Add(name, cache);
            return cache;
        }

        Cache<ProxyInt> GetIntCache(string name)
        {
            if (proxyInt.TryGetValue(name, out Cache<ProxyInt> cache))
                return cache;
            cache = new Cache<ProxyInt>(name);
            proxyInt.Add(name, cache);
            return cache;
        }

        void ResetCache()
        {
            foreach (var item in proxyFloat)
                item.Value.ResetWeight();
            foreach (var item in proxyVector)
                item.Value.ResetWeight();
            foreach (var item in proxyColor)
                item.Value.ResetWeight();
            foreach (var item in proxyInt)
                item.Value.ResetWeight();
        }

        abstract class CacheBase
        {
            public string name { get; private set; }
            public float weight0 = -1;
            public float weight1 = -1;
            public float validWeight0 => Mathf.Clamp01(weight0 * Mathf.Sign(weight0 + 1));
            public float validWeight1 => Mathf.Clamp01(weight1 * Mathf.Sign(weight1 + 1));

            public CacheBase(string name)
            {
                this.name = name;
            }

            public void ResetWeight()
            {
                weight0 = -1;
                weight1 = -1;
            }

            public void GetWeight(out float weight0, out float weight1)
            {
                weight0 = this.weight1 == -1 ? 1 : validWeight0;
                weight1 = validWeight1;
            }

            public abstract void SetVal0(Proxy proxy);
            public abstract void SetVal1(Proxy proxy);
        }

        class Cache<T> : CacheBase where T : Proxy
        {
            public T val0;
            public T val1;

            public Cache(string name) : base(name) { }

            public override void SetVal0(Proxy proxy)
            {
                val0 = proxy as T;
            }

            public override void SetVal1(Proxy proxy)
            {
                val1 = proxy as T;
            }
        }
    }
}