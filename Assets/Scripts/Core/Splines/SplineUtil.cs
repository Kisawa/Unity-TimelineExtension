using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Splines
{
    public static class SplineUtil
    {
        static void CalcSection(SplineData data, float rate, out int index0, out int index1, out float t)
        {
            index0 = -1;
            index1 = -1;
            t = 0;
            if (data == null || data.points.Count < 2)
                return;
            rate = Mathf.Clamp01(rate);
            float length = data.EuclideanLength * rate;
            int index = 0;
            float _rate = 0;
            Point last = data.points[index++];
            int count = data.Type == SplineType.Loop ? data.points.Count + 1 : data.points.Count;
            for (; index < count; index++)
            {
                Point point = data.points[index == data.points.Count ? 0 : index];
                float dis = CalcEuclideanDistance(last.point, last.forwardTangentPoint, point.backTangentPoint, point.point);
                if (length - dis < 0)
                {
                    _rate = length / dis;
                    break;
                }
                length -= dis;
                last = point;
            }
            if (index >= data.points.Count && _rate == 0)
            {
                index--;
                _rate = 1;
            }
            index0 = index - 1;
            index1 = index;
            if (index1 == data.points.Count)
                index1 = 0;
            t = _rate;
        }

        public static Vector3 GetPoint(SplineData data, float rate)
        {
            if (data == null || data.points.Count == 0)
                return Vector3.zero;
            if (data.points.Count == 1)
                return data.points[0].point;
            CalcSection(data, rate, out int index0, out int index1, out float t);
            Point point0 = data.points[index0];
            Point point1 = data.points[index1];
            return CalcBezier(point0.point, point0.forwardTangentPoint, point1.backTangentPoint, point1.point, t);
        }

        public static Vector3 GetDirection(SplineData data, float rate)
        {
            if (data == null || data.points.Count < 2)
                return Vector3.zero;
            CalcSection(data, rate, out int index0, out int index1, out float t);
            Point point0 = data.points[index0];
            Point point1 = data.points[index1];
            Vector3 dir = CalcFirstDerivative(point0.point, point0.forwardTangentPoint, point1.backTangentPoint, point1.point, t);
            return dir;
        }

        public static float CalcEuclideanLength(SplineData data)
        {
            if (data == null || data.points.Count < 2)
                return 0;
            float euclideanLength = 0;
            Point last = data.points[0];
            for (int i = 1; i < data.points.Count; i++)
            {
                Point point = data.points[i];
                euclideanLength += CalcEuclideanDistance(last.point, last.forwardTangentPoint, point.backTangentPoint, point.point);
                last = point;
            }
            if (data.Type == SplineType.Loop)
            {
                Point point = data.points[0];
                euclideanLength += CalcEuclideanDistance(last.point, last.forwardTangentPoint, point.backTangentPoint, point.point);
            }
            return euclideanLength;
        }

        public static float CalcEuclideanDistance(Vector3 point0, Vector3 tangentPoint0, Vector3 tangentPoint1, Vector3 point1)
        {
            float euclideanDis = Vector3.Distance(point0, point1);
            float appDis = Vector3.Distance(point0, tangentPoint0) + Vector3.Distance(point1, tangentPoint1) + Vector3.Distance(tangentPoint0, tangentPoint1);
            return (euclideanDis + appDis) * .5f;
        }

        public static Vector3 CalcBezier(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float t)
        {
            t = Mathf.Clamp01(t);
            float OneMinusT = 1f - t;
            return OneMinusT * OneMinusT * OneMinusT * p0 +
                3 * OneMinusT * OneMinusT * t * t0 +
                3 * OneMinusT * t * t * t1 +
                t * t * t * p1;
        }

        public static Vector3 CalcBezier(Vector3 p0, Vector3 t0, Vector3 p1, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * t0 +
                t * t * p1;
        }

        public static Vector3 CalcFirstDerivative(Vector3 p0, Vector3 t0, Vector3 p1, float t)
        {
            return 2f * (1f - t) * (t0 - p0) +
                2f * t * (p1 - t0);
        }

        public static Vector3 CalcFirstDerivative(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return 3f * oneMinusT * oneMinusT * (t0 - p0) +
                6f * oneMinusT * t * (t1 - t0) +
                3f * t * t * (p1 - t1);
        }
    }
}
