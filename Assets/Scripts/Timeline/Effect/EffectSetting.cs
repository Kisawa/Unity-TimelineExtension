using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CustomTimeline
{
    [Serializable]
    public class EffectSetting
    {
        public ActOnType actOnType;
        public ConstraintType constraintType;
        public bool fixPosition;
        public bool fixRotation;
        public bool unitScale;
        public TargetReferCoordinate targetReferCoordinate;

        public enum ActOnType { Owner, Targets, SingleTarget }
        public enum ConstraintType { Local, World, CustomWorld }
        public enum TargetReferCoordinate { TargetSelf, Owners, Relative, Absolute }
    }
}