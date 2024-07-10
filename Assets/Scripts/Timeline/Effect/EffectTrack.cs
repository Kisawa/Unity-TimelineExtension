using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [TrackColor(1, 0, .5f)]
    [TrackClipType(typeof(EffectAsset))]
    public class EffectTrack : TrackAsset 
    {
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            ((EffectAsset)clip.asset).clip = clip;
            return base.CreatePlayable(graph, gameObject, clip);
        }
    }
}