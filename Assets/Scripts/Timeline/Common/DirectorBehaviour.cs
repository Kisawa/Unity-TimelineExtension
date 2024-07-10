using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    public class DirectorBehaviour : PlayableBehaviour
    {
        PlayableDirector director;
        RhythmSetting rhythmSetting;
        bool m_SyncTime = false;
        double m_AssetDuration = double.MaxValue;

        public static ScriptPlayable<DirectorBehaviour> Create(PlayableGraph graph, PlayableDirector director, RhythmSetting rhythmSetting)
        {
            if (director == null)
                return ScriptPlayable<DirectorBehaviour>.Null;
            var handle = ScriptPlayable<DirectorBehaviour>.Create(graph);
            DirectorBehaviour behaviour = handle.GetBehaviour();
            behaviour.director = director;
            behaviour.rhythmSetting = rhythmSetting;
            return handle;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            m_SyncTime = true;
            if (director != null && director.playableAsset != null)
                m_AssetDuration = director.playableAsset.duration;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (director != null && director.playableAsset != null)
            {
                if (info.effectivePlayState == PlayState.Playing)
                    director.Pause();
                else
                    director.Stop();
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (director == null || !director.isActiveAndEnabled || director.playableAsset == null)
                return;
            // resync the time on an evaluate or a time jump (caused by loops, or some setTime calls)
            m_SyncTime |= (info.evaluationType == FrameData.EvaluationType.Evaluate) || DetectDiscontinuity(playable, info);
            SyncSpeed(info.effectiveSpeed);
            SyncStart(playable.GetGraph(), playable.GetTime());
#if !UNITY_2021_2_OR_NEWER
            SyncStop(playable.GetGraph(), playable.GetTime());
#endif
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (director == null || !director.isActiveAndEnabled || director.playableAsset == null)
                return;
            if (m_SyncTime || DetectOutOfSync(playable))
            {
                UpdateTime(playable);
                if (director.playableGraph.IsValid())
                    director.playableGraph.Evaluate();
                else
                    director.Evaluate();
            }
            m_SyncTime = false;
#if UNITY_2021_2_OR_NEWER
            SyncStop(playable.GetGraph(), playable.GetTime());
#endif
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (director != null && director.playableAsset != null)
                director.Stop();
        }

        void SyncSpeed(double speed)
        {
            if (director.playableGraph.IsValid())
            {
                int roots = director.playableGraph.GetRootPlayableCount();
                for (int i = 0; i < roots; i++)
                {
                    var rootPlayable = director.playableGraph.GetRootPlayable(i);
                    if (rootPlayable.IsValid())
                    {
                        rootPlayable.SetSpeed(speed);
                    }
                }
            }
        }

        void SyncStart(PlayableGraph graph, double time)
        {
            if (director.state == PlayState.Playing || !graph.IsPlaying() || (director.extrapolationMode == DirectorWrapMode.None && time > m_AssetDuration))
                return;
            director.Play();
        }

        void SyncStop(PlayableGraph graph, double time)
        {
            if (director.state == PlayState.Paused)
                return;
            bool expectedFinished = director.extrapolationMode == DirectorWrapMode.None && time > m_AssetDuration;
            if (expectedFinished || !graph.IsPlaying())
                director.Pause();
        }

        bool DetectDiscontinuity(Playable playable, FrameData info)
        {
            return Math.Abs(playable.GetTime() - playable.GetPreviousTime() - info.deltaTime * info.effectiveSpeed) > PlayableUtils.k_Tick;// DiscreteTime.tickValue
        }

        bool DetectOutOfSync(Playable playable)
        {
            if (rhythmSetting != null && rhythmSetting.Enable)
                return true;
            double expectedTime = playable.GetTime();
            if (playable.GetTime() >= m_AssetDuration)
            {
                switch (director.extrapolationMode)
                {
                    case DirectorWrapMode.None:
                        expectedTime = m_AssetDuration;
                        break;
                    case DirectorWrapMode.Hold:
                        expectedTime = m_AssetDuration;
                        break;
                    case DirectorWrapMode.Loop:
                        expectedTime %= m_AssetDuration;
                        break;
                }
            }
            if (!Mathf.Approximately((float)expectedTime, (float)director.time))
            {
                return true;
            }
            return false;
        }

        void UpdateTime(Playable playable)
        {
            double duration = Math.Max(0.1, director.playableAsset.duration);
            double time = playable.GetTime();
            switch (director.extrapolationMode)
            {
                case DirectorWrapMode.Hold:
                    time = Math.Min(duration, Math.Max(0, time));
                    break;
                case DirectorWrapMode.Loop:
                    time = Math.Max(0, time % duration);
                    break;
                case DirectorWrapMode.None:
                    time = Math.Min(duration, Math.Max(0, time));
                    break;
            }
            if (rhythmSetting != null)
                time = rhythmSetting.CalcRhythmTime((float)time);
            director.time = time;
        }
    }
}