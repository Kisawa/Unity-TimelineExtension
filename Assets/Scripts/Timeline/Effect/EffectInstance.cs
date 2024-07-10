using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    public abstract class EffectInstance
    {
        public Vector3 forward => TransformObjectToWorldDirection(Vector3.forward);
        public Vector3 up => TransformObjectToWorldDirection(Vector3.up);
        public Vector3 right => TransformObjectToWorldDirection(Vector3.right);

        public Transform parent;
        public GameObject gameObject;
        public Transform transform;
        public IPlayableControl[] controls;
        public IList<ParticleSystem> particles;
        public IList<PlayableDirector> directors;
        public Func<Vector3> func_OppositePositionWS;

        public Quaternion routeRotation { get; set; } = Quaternion.identity;
        public Vector3 routeScale { get; set; } = Vector3.one;

        protected Func<Vector3> func_ScaleRefer;

        public void RefreshScaleRefer(Func<Vector3> func_ScaleRefer)
        {
            this.func_ScaleRefer = func_ScaleRefer;
        }

        public abstract Vector3 TransformObjectToWorld(Vector3 positionOS);

        public abstract Vector3 TransformObjectToWorldVector(Vector3 positionOS);

        public abstract Vector3 TransformObjectToWorldDirection(Vector3 directionOS);

        public abstract Vector3 TransformWorldToObject(Vector3 positionWS);

        public abstract Vector3 TransformWorldToObjectVector(Vector3 positionWS);

        public abstract Quaternion TransformObjectToWorldRotation(Quaternion rotate);

        public abstract Vector3 GetScaleRefer();

        public Quaternion TransformObjectToWorldRotation(Vector3 eulerAngle)
        {
            return TransformObjectToWorldRotation(Quaternion.Euler(eulerAngle));
        }

        public Quaternion CorrectQuaternion(Quaternion rotation)
        {
            Vector3 scale = GetScaleRefer();
            return (Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale) * Matrix4x4.TRS(Vector3.zero, rotation, scale)).rotation;
        }

        public Vector3 CalcLocalScale(Vector3 scale)
        {
            if (func_ScaleRefer == null)
                return scale;
            Vector3 lossyScale = func_ScaleRefer.Invoke();
            lossyScale = PlayableUtils.MultiplyVector(lossyScale, scale);
            if (transform.parent == null)
                return lossyScale;
            Vector3 localScale = PlayableUtils.DivisionVector(lossyScale, transform.parent.lossyScale);
            return localScale;
        }

        public void ApplyTransform(TransformSetting setting)
        {
            if (setting == null || transform == null)
                return;
            transform.position = TransformObjectToWorld(setting.position);
            transform.rotation = TransformObjectToWorldRotation(setting.eulerAngle);
            transform.localScale = CalcLocalScale(setting.scale);
        }

#if UNITY_EDITOR
        public Quaternion editor_PlayableRouteRotation { get; set; }
#endif
    }
}