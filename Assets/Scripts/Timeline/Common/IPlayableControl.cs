using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public interface IPlayableControl
    {
        void OnGraphStart(PlayableBehaviour behaviour, Playable playable);

        void OnGraphStop(PlayableBehaviour behaviour, Playable playable);

        void OnBehaviourPlay(PlayableBehaviour behaviour, Playable playable, FrameData info);

        void OnBehaviourPause(PlayableBehaviour behaviour, Playable playable, FrameData info);

        void PrepareFrame(PlayableBehaviour behaviour, Playable playable, FrameData info);

        void ProcessFrame(PlayableBehaviour behaviour, Playable playable, FrameData info, object playerData);
    }
}