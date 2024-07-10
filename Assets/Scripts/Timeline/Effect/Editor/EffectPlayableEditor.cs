using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [CustomTimelineEditor(typeof(EffectAsset))]
    public class EffectPlayableEditor : ClipEditor
    {
        static readonly Texture2D[] s_ParticleSystemIcon = { AssetPreview.GetMiniTypeThumbnail(typeof(ParticleSystem)) };

        static readonly Color RhythmPreviewColor = new Color(.5f, .5f, .5f, .5f);
        static readonly Color ParticleSystemRhythmPreviewColor = new Color(.5f, 1, .25f, .5f);
        static readonly Color DirectorRhythmPreviewColor = new Color(.5f, .5f, 1f, .5f);
        static readonly Color BothRhythmPreviewColor = new Color(1, 0, .5f, .5f);

        static Texture2D _backgroundTex;
        static Texture2D backgroundTex
        {
            get
            {
                if (_backgroundTex == null)
                {
                    _backgroundTex = new Texture2D(1, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
                    _backgroundTex.filterMode = FilterMode.Point;
                    _backgroundTex.SetPixel(0, 0, new Color(1, 1, 1, 1));
                    _backgroundTex.Apply();
                }
                return _backgroundTex;
            }
        }

        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var asset = (EffectAsset)clip.asset;
            var options = base.GetClipOptions(clip);
            if (TimelineEditor.inspectedDirector != null && asset.instances != null && asset.instances.Count > 0)
            {
                for (int i = 0; i < asset.instances.Count; i++)
                {
                    EffectInstance instance = asset.instances[i];
                    if (instance.particles != null && instance.particles.Count > 0)
                    {
                        options.icons = s_ParticleSystemIcon;
                        break;
                    }
                }
            }
            return options;
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            EffectAsset asset = (EffectAsset)clip.asset;
            if (asset.prefab == null)
                return;
            double duration;
            bool looping = false;
            ParticleSystem[] particles = asset.prefab.GetComponentsInChildren<ParticleSystem>();
            duration = PlayableUtils.GetDurationParticleSystems(particles, ref looping);
            PlayableDirector[] directors = asset.prefab.GetComponentsInChildren<PlayableDirector>();
            duration = Math.Max(duration, PlayableUtils.GetDurationPlayableDirectors(directors, ref looping));
            asset._duration = double.IsNegativeInfinity(duration) ? PlayableBinding.DefaultDuration : duration;
            asset._looping = looping;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            EffectAsset asset = (EffectAsset)clip.asset;
            if (!asset.rhythmSetting.Enable)
                return;
            Rect rect = region.position;
            double clipDuration = clip.duration;
            double endCorrect = clip.end - clip.start - region.endTime;
            clipDuration = clipDuration - region.startTime - endCorrect;
            double rateEnd = (asset.rhythmSetting.RhythmEnd - region.startTime) / clipDuration;
            double rateStart = (asset.rhythmSetting.RhythmStart - region.startTime) / clipDuration;
            float offset = rect.width * (float)rateStart + 2;
            rect.x += offset;
            rect.width = rect.width * (float)rateEnd - offset - 2;
            float heightWeight = rect.height * .2f;
            rect.y += heightWeight * 1.4f;
            rect.height -= heightWeight * 2.2f;
            GUI.DrawTexture(rect, backgroundTex, ScaleMode.StretchToFill, true, 1, GetRhythmPreviewColor(asset), 0, 5);
        }

        public override void GetSubTimelines(TimelineClip clip, PlayableDirector director, List<PlayableDirector> subTimelines)
        {
            EffectAsset asset = (EffectAsset)clip.asset;
            if (director == null || asset.instances == null || asset.instances.Count == 0)
                return;
            for (int i = 0; i < asset.instances.Count; i++)
            {
                EffectInstance instance = asset.instances[i];
                foreach (PlayableDirector subTimeline in instance.directors)
                {
                    if (subTimeline == null || subTimeline == director || subTimeline == TimelineEditor.masterDirector)
                        continue;
                    if (subTimeline.playableAsset is TimelineAsset)
                        subTimelines.Add(subTimeline);
                }
            }
        }

        static Color GetRhythmPreviewColor(EffectAsset asset)
        {
            Color color = RhythmPreviewColor;
            if (asset == null || asset.instances == null || asset.instances.Count == 0)
                return color;
            bool particleRes = true, directorRes = true;
            for (int i = 0; i < asset.instances.Count; i++)
            {
                EffectInstance instance = asset.instances[i];
                if(!particleRes || instance.particles == null || instance.particles.Count == 0)
                    particleRes = false;
                if (!directorRes || instance.directors == null || instance.directors.Count == 0)
                    directorRes = false;
            }
            if (particleRes)
            {
                if (directorRes)
                    color = BothRhythmPreviewColor;
                else
                    color = ParticleSystemRhythmPreviewColor;
            }
            else if (directorRes)
                color = DirectorRhythmPreviewColor;
            return color;
        }
    }
}