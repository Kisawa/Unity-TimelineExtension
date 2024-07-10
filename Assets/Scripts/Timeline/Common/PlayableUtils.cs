using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    public static class PlayableUtils
    {
        public const double k_Tick = 1e-12;
        static readonly HashSet<ParticleSystem> s_SubEmitterCollector = new HashSet<ParticleSystem>();

        public static float CalcPlayableRate(Playable playable, double duration, double clipIn = 0)
        {
            double time = playable.GetTime();
            return (float)((time - clipIn) / duration);
        }

        public static AnimationCurve InvAnimationCurve(AnimationCurve curve)
        {
            if (curve == null)
                return null;
            AnimationCurve _curve = new AnimationCurve();
            Keyframe[] keyframes = curve.keys;
            for (int i = 0; i < keyframes.Length; i++)
            {
                Keyframe key = keyframes[i];
                key.time = 1 - key.time;
                float inTangent = key.inTangent;
                key.inTangent = -key.outTangent;
                key.outTangent = -inTangent;
                keyframes[i] = key;
            }
            _curve.keys = keyframes;
            return _curve;
        }

        public static Vector3 AbsVector(Vector3 vec)
        {
            vec.x = Mathf.Abs(vec.x);
            vec.y = Mathf.Abs(vec.y);
            vec.z = Mathf.Abs(vec.z);
            return vec;
        }

        public static Vector3 MultiplyVector(Vector3 vec0, Vector3 vec1)
        {
            vec0.x *= vec1.x;
            vec0.y *= vec1.y;
            vec0.z *= vec1.z;
            return vec0;
        }

        public static Vector3 DivisionVector(Vector3 vec0, Vector3 vec1)
        {
            vec0.x /= vec1.x;
            vec0.y /= vec1.y;
            vec0.z /= vec1.z;
            return vec0;
        }

        public static Vector3 UnitVector(Vector3 referVec)
        {
            Vector3 vec = Vector3.one;
            vec.x *= Mathf.Sign(referVec.x);
            vec.y *= Mathf.Sign(referVec.y);
            vec.z *= Mathf.Sign(referVec.z);
            return vec;
        }

        public static Vector3 UnitVector(Vector3 referVec, Vector3 vec)
        {
            vec = AbsVector(vec);
            vec.x *= Mathf.Sign(referVec.x);
            vec.y *= Mathf.Sign(referVec.y);
            vec.z *= Mathf.Sign(referVec.z);
            return vec;
        }

        public static Vector3 CalcSinglePositionWS(params Transform[] transforms)
        {
            Vector3 positionWS = Vector3.zero;
            if (transforms == null)
                return positionWS;
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform trans = transforms[i];
                if (trans == null)
                    continue;
                positionWS += trans.position;
            }
            positionWS /= transforms.Length;
            return positionWS;
        }

        public static Playable ConnectPlayablesToMixer(PlayableGraph graph, List<Playable> playables)
        {
            var mixer = Playable.Null;
            if (playables.Count > 0)
            {
                mixer = Playable.Create(graph, playables.Count);
                for (int i = 0; i < playables.Count; i++)
                    ConnectMixerAndPlayable(graph, mixer, playables[i], i);
                mixer.SetPropagateSetTime(true);
            }
            return mixer;
        }

        static void ConnectMixerAndPlayable(PlayableGraph graph, Playable mixer, Playable playable, int portIndex)
        {
            graph.Connect(playable, 0, mixer, portIndex);
            mixer.SetInputWeight(playable, 1);
        }

        #region PlayableDirector
        public static void ConnectDirector(IEnumerable<PlayableDirector> directors, PlayableGraph graph, List<Playable> outplayables, PlayableDirector ownerDirector, RhythmSetting rhythmSetting)
        {
            foreach (var director in directors)
            {
                if (director != null)
                {
                    if (director.playableAsset != ownerDirector)
                        outplayables.Add(DirectorBehaviour.Create(graph, director, rhythmSetting));
                    else if (director == ownerDirector)
                        director.enabled = false;
                }
            }
        }

        public static IList<PlayableDirector> GetControllablePlayableDirectors(GameObject go)
        {
            return go.GetComponentsInChildren<PlayableDirector>();
        }

        public static double GetDurationPlayableDirectors(IEnumerable<PlayableDirector> playableDirectors, ref bool looping)
        {
            double duration = double.NegativeInfinity;
            if (playableDirectors == null)
                return duration;
            foreach (PlayableDirector director in playableDirectors)
            {
                if (director.playableAsset != null)
                {
                    double assetDuration = director.playableAsset.duration;
                    if (director.playableAsset is TimelineAsset && assetDuration > 0.0)
                        assetDuration = OneTickAfter(assetDuration);
                    duration = System.Math.Max(duration, assetDuration);
                    looping |= director.extrapolationMode == DirectorWrapMode.Loop;
                }
            }
            return duration;
        }
        #endregion

        #region ParticleSystem
        public static void ConnectParticleSystem(IEnumerable<ParticleSystem> particleSystems, PlayableGraph graph, List<Playable> outplayables, ParticleSetting particleSetting, RhythmSetting rhythmSetting, float loop = 0)
        {
            foreach (var particleSystem in particleSystems)
            {
                if (particleSystem != null)
                    outplayables.Add(ParticleBehaviour.Create(graph, particleSystem, particleSetting, rhythmSetting, loop));
            }
        }

        public static IList<ParticleSystem> GetControllableParticleSystems(GameObject go)
        {
            var list = new List<ParticleSystem>();
            GetControllableParticleSystems(go.transform, list, s_SubEmitterCollector);
            s_SubEmitterCollector.Clear();
            return list;
        }

        static void GetControllableParticleSystems(Transform t, ICollection<ParticleSystem> roots, HashSet<ParticleSystem> subEmitters)
        {
            var ps = t.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (!subEmitters.Contains(ps))
                {
                    roots.Add(ps);
                    CacheSubEmitters(ps, subEmitters);
                }
            }
            for (int i = 0; i < t.childCount; ++i)
                GetControllableParticleSystems(t.GetChild(i), roots, subEmitters);
        }

        static void CacheSubEmitters(ParticleSystem ps, HashSet<ParticleSystem> subEmitters)
        {
            if (ps == null)
                return;
            for (int i = 0; i < ps.subEmitters.subEmittersCount; i++)
                subEmitters.Add(ps.subEmitters.GetSubEmitterSystem(i));
        }

        public static double GetDurationParticleSystems(IEnumerable<ParticleSystem> particleSystems, ref bool looping)
        {
            double duration = double.NegativeInfinity;
            if (particleSystems == null)
                return duration;
            foreach (ParticleSystem ps in particleSystems)
            {
                duration = System.Math.Max(duration, ps.main.duration);
                looping |= ps.main.loop;
            }
            return duration;
        }
        #endregion

        static double OneTickAfter(double time)
        {
            double number = (time / k_Tick) + 0.5;
            if (number < long.MaxValue && number > long.MinValue)
            {
                long num = (long)number + 1;
                time = num * k_Tick;
            }
            return time;
        }
    }
}