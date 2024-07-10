using CustomTimeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[ExecuteInEditMode]
public class ProxyTest : ProxyControl
{
    Renderer _renderer;
    MaterialPropertyBlock block;

    private void OnEnable()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public override void OnProxyReset()
    {
        block = new MaterialPropertyBlock();
    }

    public override void PrepareFrame(PlayableBehaviour behaviour, Playable playable, FrameData info)
    {
        base.PrepareFrame(behaviour, playable, info);
        _renderer.GetPropertyBlock(block);
    }

    public override void OnProxyUpdate(ProxyMixerBehaviour behaviour)
    {
        Color col = behaviour.GetColorProxy("col");
        block.SetColor("_EmissionColor", col);
        _renderer.SetPropertyBlock(block);
    }

    public override void OnProxyStop()
    {
        block.Clear();
        _renderer.SetPropertyBlock(block);
    }
}