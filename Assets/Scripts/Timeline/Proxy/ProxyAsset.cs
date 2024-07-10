using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    public class ProxyAsset : PlayableAsset
    {
        public List<ProxyData> proxies = new List<ProxyData>();

        public TimelineClip clip { get; set; }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<ProxyBehaviour> playable = ScriptPlayable<ProxyBehaviour>.Create(graph);
            ProxyBehaviour behaviour = playable.GetBehaviour();
            behaviour.asset = this;
            return playable;
        }
    }
}