using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    [ExecuteInEditMode]
    public abstract class ProxyControl : MonoBehaviour, IPlayableControl
    {
        public void OnGraphStart(PlayableBehaviour behaviour, Playable playable) { }

        public virtual void OnBehaviourPause(PlayableBehaviour behaviour, Playable playable, FrameData info) { }

        public virtual void OnBehaviourPlay(PlayableBehaviour behaviour, Playable playable, FrameData info)
        {
            OnProxyReset();
        }

        public virtual void PrepareFrame(PlayableBehaviour behaviour, Playable playable, FrameData info) { }

        public virtual void ProcessFrame(PlayableBehaviour behaviour, Playable playable, FrameData info, object playerData)
        {
            ProxyMixerBehaviour proxyMixerBehaviour = behaviour as ProxyMixerBehaviour;
            if (proxyMixerBehaviour != null)
                OnProxyUpdate(proxyMixerBehaviour);
        }

        public void OnGraphStop(PlayableBehaviour behaviour, Playable playable)
        {
            OnProxyStop();
        }

        public abstract void OnProxyUpdate(ProxyMixerBehaviour behaviour);
        public abstract void OnProxyReset();
        public abstract void OnProxyStop();
    }
}