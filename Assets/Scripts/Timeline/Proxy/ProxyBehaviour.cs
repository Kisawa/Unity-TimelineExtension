using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public class ProxyBehaviour : PlayableBehaviour
    {
        public ProxyAsset asset { get; set; }

        public void Reset()
        {
            for (int i = 0; i < asset.proxies.Count; i++)
                asset.proxies[i].Reset();
        }

        public float CalcPlayableRate(Playable playable)
        {
            return PlayableUtils.CalcPlayableRate(playable, asset.clip.duration, asset.clip.clipIn);
        }
    }
}