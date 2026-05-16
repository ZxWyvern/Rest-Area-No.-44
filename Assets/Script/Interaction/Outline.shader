Shader "Hidden/Outline"
{
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D_X(_OutlineBuffer);
    float4 _OutlineColor;
    float  _OutlineThickness;

    float GetMaskVal(float2 uv, float2 offset)
    {
        return SAMPLE_TEXTURE2D_X(_OutlineBuffer, s_linear_clamp_sampler, uv + offset).r;
    }

    float4 FullScreenOutline(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(
            varyings.positionCS.xy,
            _ScreenSize.zw,
            depth,
            UNITY_MATRIX_I_VP,
            UNITY_MATRIX_V
        );
        
        float2 uv = posInput.positionNDC.xy;
        float2 texelSize = _ScreenSize.zw * _OutlineThickness;

        // ── ENHANCED 9-TAP STABLE BOX SAMPLING ──
        float c      = GetMaskVal(uv, float2( 0,  0));
        
        float n      = GetMaskVal(uv, float2( 0,  1) * texelSize);
        float s      = GetMaskVal(uv, float2( 0, -1) * texelSize);
        float e      = GetMaskVal(uv, float2( 1,  0) * texelSize);
        float w      = GetMaskVal(uv, float2(-1,  0) * texelSize);
        
        float nw     = GetMaskVal(uv, float2(-1,  1) * texelSize);
        float ne     = GetMaskVal(uv, float2( 1,  1) * texelSize);
        float sw     = GetMaskVal(uv, float2(-1, -1) * texelSize);
        float se     = GetMaskVal(uv, float2( 1, -1) * texelSize);

        float sobX = (nw + 2.0 * w + sw) - (ne + 2.0 * e + se);
        float sobY = (nw + 2.0 * n + ne) - (sw + 2.0 * s + se);
        
        float edge = sqrt(sobX * sobX + sobY * sobY);

        // Perataan kontras tepi piksel diagonal mikro
        float localContrast = abs(n - c) + abs(s - c) + abs(e - c) + abs(w - c);
        edge = max(edge, localContrast * 0.5);

        // Double smoothstep clamping untuk mematikan aliasing bergetar saat kamera bergerak
        float edgeAlpha = smoothstep(0.15, 0.85, edge);
        edgeAlpha = smoothstep(0.0, 1.0, edgeAlpha);

        if (edgeAlpha <= 0.001)
        {
            return float4(0, 0, 0, 0);
        }

        return float4(_OutlineColor.rgb, _OutlineColor.a * edgeAlpha);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Name "Outline"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenOutline
            ENDHLSL
        }
    }
    Fallback Off
}