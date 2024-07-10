Shader "Project/TransparentShadowOnly"
{
    Properties
    {
        _ShadowColor("Shadow Color", Color) = (0, 0, 0)
        _ShadowStep("Shadow Step", Range(0, 1)) = 0
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        HLSLINCLUDE
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        struct Attributes
        {
            float3 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv: TEXCOORD0;
            float3 positionWS : TEXCOORD1;
        };

        CBUFFER_START(UnityPerMaterial)
        half4 _ShadowColor;
        half _ShadowStep;
        half _ShadowThreshold;
        CBUFFER_END

        Varyings vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS);
            output.uv = input.uv;
            output.positionWS = TransformObjectToWorld(input.positionOS);
            return output;
        }

        half4 frag(Varyings input) : SV_Target
        {
            half shadow = 1;
            uint meshRenderingLayers = GetMeshRenderingLayer();
            float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            Light mainLight = GetMainLight(shadowCoord);
            if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
            {
                shadow *= smoothstep(_ShadowStep, _ShadowThreshold, mainLight.shadowAttenuation);
            }

#if defined(_ADDITIONAL_LIGHTS)
            uint pixelLightCount = GetAdditionalLightsCount();
            LIGHT_LOOP_BEGIN(pixelLightCount)
            Light light = GetAdditionalLight(lightIndex, input.positionWS);
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            {
                half realtimeShadow = AdditionalLightRealtimeShadow(lightIndex, input.positionWS, light.direction);
                realtimeShadow = smoothstep(_ShadowStep, _ShadowThreshold, realtimeShadow);
                half fade = saturate(light.distanceAttenuation * Luminance(light.color));
                shadow *= lerp(1, realtimeShadow, fade);
            }
            LIGHT_LOOP_END
#endif
            half4 col = _ShadowColor;
            col.a *= 1 - shadow;
            return col;
        }
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags{ "LightMode" = "UniversalForward" }
            Cull Off ZWrite Off ZTest On Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
