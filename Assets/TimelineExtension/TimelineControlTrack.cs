using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [TrackColor(0.5f, 0, 0)]
    [TrackClipType(typeof(TimelineControlClip))]
    [TrackBindingType(typeof(MonoBehaviour))]
    public class TimelineControlTrack : TrackAsset
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);
#if UNITY_EDITOR
            TimelineControlClip _clip = clip.asset as TimelineControlClip;
            _clip.GetType().GetField("track", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(_clip, this);
#endif
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
#if UNITY_EDITOR
            foreach (var item in GetClips())
            {
                TimelineControlClip _clip = item.asset as TimelineControlClip;
                _clip.GetType().GetField("trackBinding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_clip, director.GetGenericBinding(this) as MonoBehaviour);
            }
#endif
        }
    }
}