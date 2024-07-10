using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public class EffectBehaviour : PlayableBehaviour
    {
        public EffectInstance instance { get; private set; }
        public EffectAsset asset { get; private set; }

        public static ScriptPlayable<EffectBehaviour> Create(PlayableGraph graph, EffectInstance instance, EffectAsset asset)
        {
            if (instance == null)
                return ScriptPlayable<EffectBehaviour>.Null;
            var handle = ScriptPlayable<EffectBehaviour>.Create(graph);
            EffectBehaviour behaviour = handle.GetBehaviour();
            behaviour.instance = instance;
            behaviour.asset = asset;
            return handle;
        }

        public override void OnGraphStart(Playable playable)
        {
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.OnGraphStart(this, playable);
                }
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            asset.RefreshInstanceRefer(instance);
            instance.ApplyTransform(asset.transformSetting);
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.OnBehaviourPlay(this, playable, info);
                }
            }
        }


        public override void PrepareFrame(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            instance.editor_PlayableRouteRotation = Quaternion.identity;
#endif
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.PrepareFrame(this, playable, info);
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            instance.ApplyTransform(asset.transformSetting);
            if (asset.routeSetting.enable)
            {
                RefreshRoute();
                float rate = CalcPlayableRate(playable);
                Vector3 pos = GetRoutePositionWS(rate);
                Quaternion rotation = GetRouteRotationWS(rate);
                instance.transform.position = pos;
                instance.transform.rotation = rotation;
            }
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.ProcessFrame(this, playable, info, playerData);
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.OnBehaviourPause(this, playable, info);
                }
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            if (instance.controls != null)
            {
                for (int i = 0; i < instance.controls.Length; i++)
                {
                    IPlayableControl control = instance.controls[i];
                    if (control == null)
                        continue;
                    control.OnGraphStop(this, playable);
                }
            }
        }

        public float CalcPlayableRate(Playable playable)
        {
            return PlayableUtils.CalcPlayableRate(playable, asset.clip.duration, asset.clip.clipIn);
        }

        void RefreshRoute()
        {
            instance.routeRotation = Quaternion.identity;
            instance.routeScale = Vector3.one;
            if (instance.func_OppositePositionWS != null && asset.routeSetting.constraintType != RouteSetting.ConstraintType.None)
            {
                Vector3 oppositePositionWS = instance.func_OppositePositionWS.Invoke();
                Vector3 originPositionWS = instance.TransformObjectToWorld(asset.transformSetting.position - asset.routeSetting.extendingOffset);
                switch (asset.routeSetting.constraintType)
                {
                    case RouteSetting.ConstraintType.Facing:
                    case RouteSetting.ConstraintType.FacingAndExtending:
                        Vector3 oppositeDirectionWS = asset.routeSetting.invFacing ? Vector3.Normalize(originPositionWS - oppositePositionWS) : Vector3.Normalize(oppositePositionWS - originPositionWS);
                        instance.routeRotation = Quaternion.FromToRotation(instance.forward, oppositeDirectionWS) * Quaternion.Euler(asset.routeSetting.facingRotation);
                        break;
                }
                switch (asset.routeSetting.constraintType)
                {
                    case RouteSetting.ConstraintType.Extending:
                    case RouteSetting.ConstraintType.FacingAndExtending:
                        if (asset.routeSetting.route.points.Count > 0)
                        {
                            float distance = Vector3.Distance(oppositePositionWS, originPositionWS);
                            Vector3 endPoint = instance.TransformObjectToWorldVector(asset.routeSetting.route.points[asset.routeSetting.route.points.Count - 1].point);
                            float routeDistance = endPoint.magnitude;
                            instance.routeScale = routeDistance == 0 ? Vector3.zero : Vector3.one * distance / routeDistance;
                        }
                        break;
                }
            }
        }

        Vector3 GetRoutePositionWS(float rate)
        {
            Vector3 pos = asset.routeSetting.GetPositionOS(rate);
            pos = PlayableUtils.MultiplyVector(pos, instance.routeScale);
            pos = instance.routeRotation * instance.TransformObjectToWorldVector(pos);
            return pos + instance.TransformObjectToWorld(asset.transformSetting.position);
        }

        Quaternion GetRouteRotationWS(float rate)
        {
            Vector3 dir = asset.routeSetting.GetDirectionOS(rate);
            dir = PlayableUtils.MultiplyVector(dir, instance.routeScale);
            Quaternion lookAtRotate = instance.routeRotation * Quaternion.FromToRotation(instance.forward, instance.TransformObjectToWorldDirection(dir.normalized));
#if UNITY_EDITOR
            instance.editor_PlayableRouteRotation = lookAtRotate;
#endif
            Quaternion rotate = instance.TransformObjectToWorldRotation(asset.transformSetting.eulerAngle);
            return lookAtRotate * rotate;
        }
    }
}