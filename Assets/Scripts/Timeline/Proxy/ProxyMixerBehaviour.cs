using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public partial class ProxyMixerBehaviour : PlayableBehaviour
    {
        public IPlayableControl control { get; set; }

        public override void OnGraphStart(Playable playable)
        {
            control.OnGraphStart(this, playable);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            for (int i = 0; i < playable.GetInputCount(); i++)
            {
                ScriptPlayable<ProxyBehaviour> handle = (ScriptPlayable<ProxyBehaviour>)playable.GetInput(i);
                ProxyBehaviour behaviour = handle.GetBehaviour();
                behaviour.Reset();
            }
            control.OnBehaviourPlay(this, playable, info);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            ResetCache();
            control.PrepareFrame(this, playable, info);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            const int round = 7;
            double time = Math.Round(playable.GetTime(), round);
            for (int i = 0; i < playable.GetInputCount(); i++)
            {
                ScriptPlayable<ProxyBehaviour> handle = (ScriptPlayable<ProxyBehaviour>)playable.GetInput(i);
                ProxyBehaviour behaviour = handle.GetBehaviour();
                float weight = playable.GetInputWeight(i);
                if (time < Math.Round(behaviour.asset.clip.start, round))
                {
                    for (int j = 0; j < behaviour.asset.proxies.Count; j++)
                    {
                        ProxyData data = behaviour.asset.proxies[j];
                        CacheBase cache = GetCache(data);
                        if (cache == null)
                            continue;
                        if (cache.weight0 == -1)
                        {
                            data.Apply(0);
                            cache.weight0 = -2;
                            cache.SetVal0(data.proxy);
                        }
                    }
                }
                else if (time < Math.Round(behaviour.asset.clip.end, round))
                {
                    float rate = behaviour.CalcPlayableRate(handle);
                    for (int j = 0; j < behaviour.asset.proxies.Count; j++)
                    {
                        ProxyData data = behaviour.asset.proxies[j];
                        CacheBase cache = GetCache(data);
                        if (cache == null)
                            continue;
                        if (cache.weight0 < 0 || cache.weight0 > 1)
                        {
                            data.Apply(rate);
                            cache.weight0 = weight;
                            cache.SetVal0(data.proxy);
                        }
                        else if (cache.weight1 < 0 || cache.weight1 > 1)
                        {
                            data.Apply(rate);
                            cache.weight1 = weight;
                            cache.SetVal1(data.proxy);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < behaviour.asset.proxies.Count; j++)
                    {
                        ProxyData data = behaviour.asset.proxies[j];
                        CacheBase cache = GetCache(data);
                        if (cache == null)
                            continue;
                        if (cache.weight0 < 0 || cache.weight0 > 1)
                        {
                            data.Apply(1);
                            cache.weight0 = 2;
                            cache.SetVal0(data.proxy);
                        }
                    }
                }
            }
            control.ProcessFrame(this, playable, info, playerData);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            control.OnBehaviourPause(this, playable, info);
        }

        public override void OnGraphStop(Playable playable)
        {
            control.OnGraphStop(this, playable);
        }

        public float GetFloatProxy(string name)
        {
            if (!proxyFloat.TryGetValue(name, out Cache<ProxyFloat> cache))
                return 0;
            float weight0 = cache.weight1 == -1 ? 1 : cache.validWeight0;
            float weight1 = cache.validWeight1;
            float val = 0;
            if (cache.val0 != null)
                val += cache.val0.Val * weight0;
            if (cache.val1 != null)
                val += cache.val1.Val * weight1;
            return val;
        }

        public Vector4 GetVectorProxy(string name)
        {
            if (!proxyVector.TryGetValue(name, out Cache<ProxyVector> cache))
                return Vector4.zero;
            cache.GetWeight(out float weight0, out float weight1);
            Vector4 val = Vector4.zero;
            if (cache.val0 != null)
                val += cache.val0.Val * weight0;
            if (cache.val1 != null)
                val += cache.val1.Val * weight1;
            return val;
        }

        public Color GetColorProxy(string name)
        {
            if (!proxyColor.TryGetValue(name, out Cache<ProxyColor> cache))
                return Color.clear;
            cache.GetWeight(out float weight0, out float weight1);
            Color val = Color.clear;
            if (cache.val0 != null)
                val += cache.val0.Val * weight0;
            if (cache.val1 != null)
                val += cache.val1.Val * weight1;
            return val;
        }

        public int GetIntProxy(string name)
        {
            if (!proxyInt.TryGetValue(name, out Cache<ProxyInt> cache))
                return 0;
            cache.GetWeight(out float weight0, out float weight1);
            float val = 0;
            if (cache.val0 != null)
                val += cache.val0.Val * weight0;
            if (cache.val1 != null)
                val += cache.val1.Val * weight1;
            return Mathf.RoundToInt(val);
        }
    }
}