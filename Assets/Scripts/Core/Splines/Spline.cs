using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public class Spline : MonoBehaviour
    {
        public Vector3 offset;
        public SplineData data = new SplineData();

        Transform trans;

        private void Awake()
        {
            trans = transform;
        }

        public Vector3 GetPoint(float rate)
        {
            if (data == null)
                return CalcPositionWS(Vector3.zero);
            Vector3 point = SplineUtil.GetPoint(data, rate);
            return CalcPositionWS(point);
        }

        public Vector3 GetDirection(float rate)
        {
            if(data == null)
                return TransformDirection(Vector3.zero);
            Vector3 dir = SplineUtil.GetDirection(data, rate);
            return TransformDirection(dir);
        }

        public Vector3 TransformDirection(Vector3 dir)
        {
            Vector3 scale = trans.lossyScale;
            dir.x *= scale.x;
            dir.y *= scale.y;
            dir.z *= scale.z;
            return trans.TransformDirection(dir.normalized);
        }

        public Vector3 CalcPositionWS(Vector3 point)
        {
            return trans.TransformPoint(point + offset);
        }
    }
}