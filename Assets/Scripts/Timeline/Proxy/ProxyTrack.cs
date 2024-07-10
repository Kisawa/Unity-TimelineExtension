using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [TrackColor(1, 1, 1)]
    [TrackClipType(typeof(ProxyAsset))]
    [TrackBindingType(typeof(MonoBehaviour))]
    public class ProxyTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector director = graph.GetResolver() as PlayableDirector;
            IPlayableControl control = director.GetGenericBinding(this) as IPlayableControl;
            if (control == null)
                return Playable.Create(graph, inputCount);
            var playable = ScriptPlayable<ProxyMixerBehaviour>.Create(graph, inputCount);
            ProxyMixerBehaviour behaviour = playable.GetBehaviour();
            behaviour.control = control;
            foreach (var item in GetClips())
            {
                ProxyAsset asset = item.asset as ProxyAsset;
                asset.clip = item;
            }
            return playable;
        }
    }
}