using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [Serializable]
    public class EffectAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | (_looping ? ClipCaps.Looping : ClipCaps.None);
        public override double duration => _duration;
        public double _duration { get; set; } = PlayableBinding.DefaultDuration;
        public bool _looping { get; set; }

        public GameObject prefab;
        public string instantiateTransPath;
        public EffectSetting effectSetting = new EffectSetting();
        public TransformSetting transformSetting = new TransformSetting();
        public ParticleSetting particleSetting = new ParticleSetting();
        public RhythmSetting rhythmSetting = new RhythmSetting();
        public RouteSetting routeSetting = new RouteSetting();

        public TimelineClip clip { get; set; }
        public GameObject owner { get; private set; }
        public PlayableDirector ownerDirector { get; private set; }
        public CustomTimelineCore autoTimeline { get; private set; }
        public List<EffectInstance> instances { get; private set; }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            this.owner = owner;
            ownerDirector = owner.GetComponent<PlayableDirector>();
            autoTimeline = owner.GetComponent<CustomTimelineCore>();
            if (prefab == null)
            {
                instances = null;
                _duration = PlayableBinding.DefaultDuration;
                _looping = false;
                return Playable.Create(graph);
            }

            List<Playable> playables = new List<Playable>();
            instances = new List<EffectInstance>();
            switch (effectSetting.actOnType)
            {
                case EffectSetting.ActOnType.Owner:
                    CreateOwnerInstance(graph, playables);
                    break;
                case EffectSetting.ActOnType.Targets:
                    CreateTargetsInstance(graph, playables);
                    break;
                case EffectSetting.ActOnType.SingleTarget:
                    CreateSingleTargetInstance(graph, playables);
                    break;
            }
            Playable root = PlayableUtils.ConnectPlayablesToMixer(graph, playables);
            if (!root.IsValid())
                root = Playable.Create(graph);
            return root;
        }

        public void RefreshInstanceRefer(EffectInstance instance)
        {
            if (instance == null)
                return;
            switch (effectSetting.actOnType)
            {
                case EffectSetting.ActOnType.Owner:
                    RefreshOwnerInstanceRefer(instance);
                    break;
                case EffectSetting.ActOnType.Targets:
                    RefreshTargetInstanceRefer(instance);
                    break;
                case EffectSetting.ActOnType.SingleTarget:
                    Transform[] targets = GetTargets();
                    RefreshSingleTargetInstanceRefer(instance, targets);
                    break;
            }
        }

        void CreateOwnerInstance(PlayableGraph graph, List<Playable> playables)
        {
            Transform parent = GetParentTransform(owner.transform);
            EffectInstance instance = new EffectInstanceMatrix();
            instance.parent = parent;
            RefreshOwnerInstanceRefer(instance);
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.World:
                case EffectSetting.ConstraintType.CustomWorld:
                    parent = null;
                    break;
            }
            ConnectInstance(instance, graph, playables, parent);
        }

        void CreateTargetsInstance(PlayableGraph graph, List<Playable> playables)
        {
            Transform[] targets = GetTargets();
            if (targets == null)
                return;
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                if (target == null)
                    continue;
                Transform parent = GetParentTransform(target);
                EffectInstance instance = new EffectInstanceMatrix();
                instance.parent = parent;
                RefreshTargetInstanceRefer(instance);
                switch (effectSetting.constraintType)
                {
                    case EffectSetting.ConstraintType.World:
                    case EffectSetting.ConstraintType.CustomWorld:
                        parent = null;
                        break;
                }
                ConnectInstance(instance, graph, playables, parent);
            }
        }

        void CreateSingleTargetInstance(PlayableGraph graph, List<Playable> playables)
        {
            Transform[] targets = GetTargets();
            if (targets == null || targets.Length == 0)
                return;
            EffectInstanceMatrix instance = new EffectInstanceMatrix();
            RefreshSingleTargetInstanceRefer(instance, targets);
            ConnectInstance(instance, graph, playables, null);
        }

        void ConnectInstance(EffectInstance instance, PlayableGraph graph, List<Playable> playables, Transform parent)
        {
            if (instance == null)
                return;
            ScriptPlayable<PrefabControlPlayable> controlPlayable = PrefabControlPlayable.Create(graph, prefab, parent);
            instance.gameObject = controlPlayable.GetBehaviour().prefabInstance;
            instance.transform = instance.gameObject.transform;
            MonoBehaviour[] monos = instance.gameObject.GetComponents<MonoBehaviour>();
            List<IPlayableControl> controlList = new List<IPlayableControl>();
            for (int i = 0; i < monos.Length; i++)
            {
                IPlayableControl control = monos[i] as IPlayableControl;
                if(control != null)
                    controlList.Add(control);
            }
            instance.controls = controlList.ToArray();
            playables.Add(controlPlayable);
            playables.Add(EffectBehaviour.Create(graph, instance, this));
            ConnectEffects(instance, graph, playables);
            instances.Add(instance);
        }

        void ConnectEffects(EffectInstance instance, PlayableGraph graph, List<Playable> playables)
        {
            double duration;
            bool looping = false;
            instance.particles = PlayableUtils.GetControllableParticleSystems(instance.gameObject);
            duration = PlayableUtils.GetDurationParticleSystems(instance.particles, ref looping);
            float particleLoop = looping ? (float)duration : 0;
            PlayableUtils.ConnectParticleSystem(instance.particles, graph, playables, particleSetting, rhythmSetting, particleLoop);

            instance.directors = PlayableUtils.GetControllablePlayableDirectors(instance.gameObject);
            for (int i = 0; i < instance.directors.Count; i++)
            {
                CustomTimelineCore auto = instance.directors[i].GetComponent<CustomTimelineCore>();
                if (auto != null)
                    auto.CoreProxy = autoTimeline;
            }
            duration = Math.Max(duration, PlayableUtils.GetDurationPlayableDirectors(instance.directors, ref looping));
            PlayableUtils.ConnectDirector(instance.directors, graph, playables, ownerDirector, rhythmSetting);

            _duration = double.IsNegativeInfinity(duration) ? PlayableBinding.DefaultDuration : duration;
            _looping = looping;
        }

        void RefreshOwnerInstanceRefer(EffectInstance instance)
        {
            if (instance == null || instance.parent == null)
                return;
            instance.RefreshScaleRefer(GetFunc_ScaleRefer(instance.parent));
            EffectInstanceMatrix effectInstance = instance as EffectInstanceMatrix;
            if (effectInstance == null)
                return;
            effectInstance.RefreshPositionWSRefer(GetFunc_PositionWSRefer(instance.parent));
            effectInstance.RefreshRotationWSRefer(GetFunc_RotationWSRefer(instance.parent));
            effectInstance.func_OppositePositionWS = null;
        }

        void RefreshTargetInstanceRefer(EffectInstance instance)
        {
            if (instance == null || instance.parent == null)
                return;
            Transform ownerTrans = GetParentTransform(owner.transform);
            Vector3 oppositePositionWS = ownerTrans.position;
            instance.func_OppositePositionWS = routeSetting.lockOpposite ? () => ownerTrans.position : () => oppositePositionWS;
            EffectInstanceMatrix effectInstance = instance as EffectInstanceMatrix;
            if (effectInstance == null)
                return;
            switch (effectSetting.targetReferCoordinate)
            {
                case EffectSetting.TargetReferCoordinate.TargetSelf:
                    effectInstance.RefreshPositionWSRefer(GetFunc_PositionWSRefer(instance.parent));
                    effectInstance.RefreshRotationWSRefer(GetFunc_RotationWSRefer(instance.parent));
                    effectInstance.RefreshScaleRefer(GetFunc_ScaleRefer(instance.parent, instance.parent.lossyScale));
                    break;
                case EffectSetting.TargetReferCoordinate.Owners:
                    effectInstance.RefreshPositionWSRefer(GetFunc_PositionWSRefer(instance.parent));
                    effectInstance.RefreshRotationWSRefer(GetFunc_RotationWSRefer(ownerTrans));
                    effectInstance.RefreshScaleRefer(GetFunc_ScaleRefer(ownerTrans, ownerTrans.lossyScale));
                    break;
                case EffectSetting.TargetReferCoordinate.Relative:
                    Func<Vector3> func_ScaleRefer = null;
                    switch (effectSetting.constraintType)
                    {
                        case EffectSetting.ConstraintType.Local:
                            func_ScaleRefer = () => PlayableUtils.UnitVector(ownerTrans.lossyScale, instance.parent.lossyScale);
                            break;
                        case EffectSetting.ConstraintType.World:
                            {
                                Vector3 scale = PlayableUtils.AbsVector(instance.parent.lossyScale);
                                func_ScaleRefer = () => scale;
                            }
                            break;
                        case EffectSetting.ConstraintType.CustomWorld:
                            {
                                Vector3 scale = PlayableUtils.AbsVector(instance.parent.lossyScale);
                                Vector3 unitScale = PlayableUtils.UnitVector(ownerTrans.lossyScale, scale);
                                func_ScaleRefer = effectSetting.unitScale ? () => unitScale : () => scale;
                            }
                            break;
                    }
                    effectInstance.RefreshPositionWSRefer(GetFunc_PositionWSRefer(instance.parent));
                    effectInstance.RefreshRotationWSRefer(GetFunc_RotationWSRefer(ownerTrans));
                    effectInstance.RefreshScaleRefer(func_ScaleRefer);
                    break;
                case EffectSetting.TargetReferCoordinate.Absolute:
                    effectInstance.RefreshPositionWSRefer(GetFunc_PositionWSRefer(instance.parent));
                    effectInstance.RefreshRotationWSRefer(() => Quaternion.identity);
                    effectInstance.RefreshScaleRefer(() => Vector3.one);
                    break;
            }
        }

        void RefreshSingleTargetInstanceRefer(EffectInstance instance, Transform[] targets)
        {
            if (instance == null || targets == null || targets.Length == 0)
                return;
            Transform ownerTrans = GetParentTransform(owner.transform);
            Vector3 oppositePositionWS = ownerTrans.position;
            instance.func_OppositePositionWS = routeSetting.lockOpposite ? () => ownerTrans.position : () => oppositePositionWS;
            EffectInstanceMatrix effectInstance = instance as EffectInstanceMatrix;
            if (effectInstance == null)
                return;
            Func<Vector3> func_PositionWSRefer = null;
            Func<Quaternion> func_RotationWSRefer = null;
            Func<Vector3> func_ScaleRefer = null;
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.Local:
                    func_PositionWSRefer = () => PlayableUtils.CalcSinglePositionWS(targets);
                    break;
                case EffectSetting.ConstraintType.World:
                    {
                        Vector3 positionWS = PlayableUtils.CalcSinglePositionWS(targets);
                        func_PositionWSRefer = () => positionWS;
                    }
                    break;
                case EffectSetting.ConstraintType.CustomWorld:
                    {
                        Vector3 positionWS = PlayableUtils.CalcSinglePositionWS(targets);
                        func_PositionWSRefer = effectSetting.fixPosition ? () => PlayableUtils.CalcSinglePositionWS(targets) : () => positionWS;
                    }
                    break;
            }
            switch (effectSetting.targetReferCoordinate)
            {
                case EffectSetting.TargetReferCoordinate.TargetSelf:
                    switch (effectSetting.constraintType)
                    {
                        case EffectSetting.ConstraintType.Local:
                            func_RotationWSRefer = () => Quaternion.Inverse(ownerTrans.rotation);
                            break;
                        case EffectSetting.ConstraintType.World:
                            {
                                Quaternion rotation = Quaternion.Inverse(ownerTrans.rotation);
                                func_RotationWSRefer = () => rotation;
                            }
                            break;
                        case EffectSetting.ConstraintType.CustomWorld:
                            {
                                Quaternion rotation = Quaternion.Inverse(ownerTrans.rotation);
                                func_RotationWSRefer = effectSetting.fixRotation ? () => Quaternion.Inverse(ownerTrans.rotation) : () => rotation;
                            }
                            break;
                    }
                    func_ScaleRefer = GetFunc_ScaleRefer(ownerTrans, ownerTrans.lossyScale);
                    break;
                case EffectSetting.TargetReferCoordinate.Owners:
                    func_RotationWSRefer = GetFunc_RotationWSRefer(ownerTrans);
                    func_ScaleRefer = GetFunc_ScaleRefer(ownerTrans, ownerTrans.lossyScale);
                    break;
                case EffectSetting.TargetReferCoordinate.Relative:
                    func_RotationWSRefer = GetFunc_RotationWSRefer(ownerTrans);
                    switch (effectSetting.constraintType)
                    {
                        case EffectSetting.ConstraintType.Local:
                            func_ScaleRefer = () => PlayableUtils.UnitVector(ownerTrans.lossyScale);
                            break;
                        case EffectSetting.ConstraintType.World:
                            func_ScaleRefer = () => Vector3.one;
                            break;
                        case EffectSetting.ConstraintType.CustomWorld:
                            Vector3 unitScale = PlayableUtils.UnitVector(ownerTrans.lossyScale);
                            func_ScaleRefer = effectSetting.unitScale ? () => unitScale : () => Vector3.one;
                            break;
                    }
                    break;
                case EffectSetting.TargetReferCoordinate.Absolute:
                    func_RotationWSRefer = () => Quaternion.identity;
                    func_ScaleRefer = () => Vector3.one;
                    break;
            }
            effectInstance.RefreshPositionWSRefer(func_PositionWSRefer);
            effectInstance.RefreshRotationWSRefer(func_RotationWSRefer);
            effectInstance.RefreshScaleRefer(func_ScaleRefer);
        }

        public Transform GetParentTransform(Transform trans)
        {
            if (trans == null || string.IsNullOrEmpty(instantiateTransPath))
                return trans;
            Transform _trans = trans.Find(instantiateTransPath);
            if(_trans == null)
                return trans;
            return _trans;
        }

        Func<Vector3> GetFunc_PositionWSRefer(Transform trans)
        {
            Func<Vector3> func = null;
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.Local:
                    func = () => trans.position;
                    break;
                case EffectSetting.ConstraintType.World:
                    {
                        Vector3 position = trans.position;
                        func = () => position;
                    }
                    break;
                case EffectSetting.ConstraintType.CustomWorld:
                    {
                        Vector3 position = trans.position;
                        func = effectSetting.fixPosition ? () => trans.position : () => position;
                    }
                    break;
            }
            return func;
        }

        Func<Quaternion> GetFunc_RotationWSRefer(Transform trans)
        {
            Func<Quaternion> func = null;
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.Local:
                    func = () => trans.rotation;
                    break;
                case EffectSetting.ConstraintType.World:
                    {
                        Quaternion rotation = trans.rotation;
                        func = () => rotation;
                    }
                    break;
                case EffectSetting.ConstraintType.CustomWorld:
                    {
                        Quaternion rotation = trans.rotation;
                        func = effectSetting.fixRotation ? () => trans.rotation : () => rotation;
                    }
                    break;
            }
            return func;
        }

        Func<Vector3> GetFunc_ScaleRefer(Transform trans)
        {
            Func<Vector3> func = null;
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.Local:
                    func = () => trans.lossyScale;
                    break;
                case EffectSetting.ConstraintType.World:
                    func = () => Vector3.one;
                    break;
                case EffectSetting.ConstraintType.CustomWorld:
                    Vector3 scale = PlayableUtils.UnitVector(trans.lossyScale);
                    func = effectSetting.unitScale ? () => scale : () => Vector3.one;
                    break;
            }
            return func;
        }

        Func<Vector3> GetFunc_ScaleRefer(Transform trans, Vector3 worldReferScale)
        {
            Func<Vector3> func = null;
            switch (effectSetting.constraintType)
            {
                case EffectSetting.ConstraintType.Local:
                    func = () => trans.lossyScale;
                    break;
                case EffectSetting.ConstraintType.World:
                    worldReferScale = PlayableUtils.AbsVector(worldReferScale);
                    func = () => worldReferScale;
                    break;
                case EffectSetting.ConstraintType.CustomWorld:
                    worldReferScale = PlayableUtils.AbsVector(worldReferScale);
                    Vector3 unitScale = PlayableUtils.UnitVector(trans.lossyScale, worldReferScale);
                    func = effectSetting.unitScale ? () => unitScale : () => worldReferScale;
                    break;
            }
            return func;
        }

        Transform[] GetTargets()
        {
            if (autoTimeline == null)
                return null;
            return autoTimeline.GetTargets();
        }
    }
}