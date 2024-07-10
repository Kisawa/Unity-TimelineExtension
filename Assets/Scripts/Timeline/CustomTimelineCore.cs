using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace CustomTimeline
{
    [RequireComponent(typeof(PlayableDirector)), DisallowMultipleComponent]
    public class CustomTimelineCore : MonoBehaviour
    {
        public CustomTimelineCore CoreProxy { get; set; }

        [SerializeField] Transform[] Targets;

        public Transform[] GetTargets()
        {
            if (CoreProxy == null)
                return Targets;
            return CoreProxy.GetTargets();
        }
    }
}