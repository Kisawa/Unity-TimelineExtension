using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [System.Serializable]
    public class SplineData
    {
        [SerializeField] SplineType type;
        [SerializeField] float euclideanLength;
        public List<Point> points = new List<Point>();

        public SplineType Type => type;
        public float EuclideanLength => euclideanLength;

        public void RefreshEuclideanLength()
        {
            euclideanLength = SplineUtil.CalcEuclideanLength(this);
        }

#if UNITY_EDITOR
        [SerializeField] List<EditorPointExtra> editorPointsExtra = new List<EditorPointExtra>();
#endif
    }

#if UNITY_EDITOR
    [System.Serializable]
    public struct EditorPointExtra
    {
        public TangentHandleType tangentHandleType;
        public Quaternion rotation;
        public Quaternion forwardTangentRotation;
        public Quaternion backTangentRotation;
    }

    public enum TangentHandleType { Free, Aligned, Mirror }
#endif

    [System.Serializable]
    public struct Point
    {
        public Vector3 point;
        public Vector3 forwardTangentOffset;
        public Vector3 backTangentOffset;

        public Vector3 forwardTangentPoint => point + forwardTangentOffset;
        public Vector3 backTangentPoint => point + backTangentOffset;
    }
}