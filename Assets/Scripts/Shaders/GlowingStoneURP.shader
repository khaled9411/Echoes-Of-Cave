// A simple glowing shader for URP projects.
// This shader allows you to add an emissive glow to the main texture.

Shader "Custom/GlowingStoneURP"
{
    // Properties that appear in the Material Inspector.
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionIntensity("Emission Intensity", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            // Shader definitions.
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Input data structure from the model.
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Output from the Vertex Shader, input to the Fragment Shader.
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Shader variables.
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _EmissionIntensity;

            // Vertex Shader.
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            // Fragment Shader.
            float4 frag(Varyings input) : SV_TARGET
            {
                // Read color from the main texture.
                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;

                // Calculate the emissive glow color.
                // We add the emission color to the main color.
                float4 emission = _EmissionColor * _EmissionIntensity;

                // The final color is the main color plus the emission.
                float4 finalColor = mainColor + emission;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
