Shader "EighthSin/FullscreenPixelation"
{
    Properties
    {
        _PixelSize ("Pixel Count", Float) = 400
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        Cull Off

        Pass
        {
            Name "FullscreenPixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelSize;

            half4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float pixelCount = max(_PixelSize, 1.0);
                float2 uv = floor(input.texcoord.xy * pixelCount) / pixelCount;

                // Full Screen Pass의 복사본을 읽으므로 투명 오브젝트도 포함된다.
                return SAMPLE_TEXTURE2D_X_LOD(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv,
                    _BlitMipLevel
                );
            }
            ENDHLSL
        }
    }
}
