using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public class ParticleBehaviour : PlayableBehaviour
    {
        const float kUnsetTime = float.MaxValue;

        ParticleSystem particle;
        RhythmSetting rhythmSetting;
        float loop;
        float m_LastPlayableTime = kUnsetTime;
        float m_LastParticleTime = kUnsetTime;

        public static ScriptPlayable<ParticleBehaviour> Create(PlayableGraph graph, ParticleSystem component, ParticleSetting particleSetting, RhythmSetting rhythmSetting, float loop = 0)
        {
            if (component == null)
                return ScriptPlayable<ParticleBehaviour>.Null;
            var handle = ScriptPlayable<ParticleBehaviour>.Create(graph);
            ParticleBehaviour behaviour = handle.GetBehaviour();
            behaviour.particle = component;
            particleSetting.ApplyRandomSeed(component);
            behaviour.rhythmSetting = rhythmSetting;
            behaviour.loop = loop;
            return handle;
        }

        public override void PrepareFrame(Playable playable, FrameData data)
        {
            if (particle == null || !particle.gameObject.activeInHierarchy)
            {
                m_LastPlayableTime = kUnsetTime;
                return;
            }
            float time = (float)playable.GetTime();
            if (rhythmSetting != null)
            {
                if (loop > 0 && loop >= rhythmSetting.RhythmEnd && time > loop)
                {
                    float count = Mathf.FloorToInt(time / loop);
                    float span = time % loop;
                    time = count * loop + rhythmSetting.CalcRhythmTime(span);
                }
                else
                    time = rhythmSetting.CalcRhythmTime(time);
            }
            float particleTime = particle.time;

            if (m_LastPlayableTime > time || !Mathf.Approximately(particleTime, m_LastParticleTime))
                Simulate(time, true);
            else if (m_LastPlayableTime < time)
                Simulate(time - m_LastPlayableTime, false);

            m_LastPlayableTime = time;
            m_LastParticleTime = particle.time;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            m_LastPlayableTime = kUnsetTime;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            m_LastPlayableTime = kUnsetTime;
        }

        void Simulate(float time, bool restart)
        {
            const bool withChildren = false;
            const bool fixedTimeStep = false;
            float maxTime = Time.maximumDeltaTime;
            if (restart)
                particle.Simulate(0, withChildren, true, fixedTimeStep);
            while (time > maxTime)
            {
                particle.Simulate(maxTime, withChildren, false, fixedTimeStep);
                time -= maxTime;
            }
            if (time > 0)
                particle.Simulate(time, withChildren, false, fixedTimeStep);
        }
    }
}