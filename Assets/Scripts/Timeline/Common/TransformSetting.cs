using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTimeline
{
    [Serializable]
    public class TransformSetting
    {
        public Vector3 position;
        public Vector3 eulerAngle;
        public Vector3 scale = Vector3.one;
    }
}