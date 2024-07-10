using System;
using UnityEngine;

namespace CustomTimeline
{
    public class EffectInstanceMatrix : EffectInstance
    {
        Func<Vector3> func_PositionWSRefer;
        Func<Quaternion> func_RotationWSRefer;

        public void RefreshPositionWSRefer(Func<Vector3> func_PositionWSRefer)
        {
            this.func_PositionWSRefer = func_PositionWSRefer;
        }

        public void RefreshRotationWSRefer(Func<Quaternion> func_RotationWSRefer)
        {
            this.func_RotationWSRefer = func_RotationWSRefer;
        }

        public override Vector3 TransformObjectToWorld(Vector3 positionOS)
        {
            Vector4 pos = positionOS;
            pos.w = 1;
            return localToWorld() * pos;
        }

        public override Vector3 TransformObjectToWorldDirection(Vector3 directionOS)
        {
            Matrix4x4 mat = localToWorld();
            mat.SetRow(3, Vector4.zero);
            return (mat * directionOS).normalized;
        }

        public override Quaternion TransformObjectToWorldRotation(Quaternion rotate)
        {
            return (localToWorld() * Matrix4x4.TRS(Vector3.zero, rotate, GetScaleRefer())).rotation;
        }

        public override Vector3 TransformObjectToWorldVector(Vector3 positionOS)
        {
            return localToWorld() * positionOS;
        }

        public override Vector3 TransformWorldToObject(Vector3 positionWS)
        {
            Vector4 pos = positionWS;
            pos.w = 1;
            return localToWorld().inverse * pos;
        }

        public override Vector3 TransformWorldToObjectVector(Vector3 positionWS)
        {
            return localToWorld().inverse * positionWS;
        }

        public Matrix4x4 localToWorld()
        {
            Vector3 positionWS = func_PositionWSRefer == null ? Vector3.zero : func_PositionWSRefer.Invoke();
            Quaternion rotationWS = func_RotationWSRefer == null ? Quaternion.identity : func_RotationWSRefer.Invoke();
            Vector3 scale = GetScaleRefer();
            return Matrix4x4.TRS(positionWS, rotationWS, scale);
        }

        public override Vector3 GetScaleRefer()
        { 
            return func_ScaleRefer == null ? Vector3.one : func_ScaleRefer.Invoke();
        }
    }
}