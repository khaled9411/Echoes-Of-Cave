Shader "Custom/SpriteLitFadeURP"
{
    Properties
    {
        [PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        _EmissionColor("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity("Emission Intensity", Range(0,10)) = 1
        _MinAlpha("Minimum Alpha", Range(0,1)) = 0
        _FadeStrength("Light Fade Strength", Range(0,5)) = 2
        _GlowStrength("Glow Strength Near Light", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }

        // Render both sides
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // URP lighting variants
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _EmissionColor;
                float  _EmissionIntensity;
                float  _MinAlpha;
                float  _FadeStrength;
                float  _GlowStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color      = IN.color * _Color;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            // FRONT_FACE macros come from Core.hlsl:
            // FRONT_FACE_TYPE typically 'bool', FRONT_FACE_SEMANTIC is SV_IsFrontFace
            float4 frag(Varyings IN, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // Sprite default normal is -Z in object space. Transform to world and handle negative scale.
                float3 normalWS = normalize(TransformObjectToWorldDir(float3(0,0,-1)));
                normalWS *= GetOddNegativeScale();

                // Flip normal for backfaces so both sides light consistently
                if (!isFrontFace) normalWS = -normalWS;

                // -------- Main light --------
                Light  mainLight = GetMainLight();
                float3 Lm        = normalize(mainLight.direction);
                float  NdotLm    = saturate(dot(normalWS, Lm));
                float3 mainCol   = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                float  mainInt   = NdotLm * Luminance(mainCol);

                // -------- Additional lights (point/spot) --------
                float3 addDiffuse = 0.0;
                float  addInt     = 0.0;

                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                [loop]
                for (uint i = 0u; i < count; i++)
                {
                    Light  addL   = GetAdditionalLight(i, IN.positionWS);
                    float3 La     = normalize(addL.direction);
                    float  NdotLa = saturate(dot(normalWS, La));
                    float3 addCol = addL.color * addL.distanceAttenuation * addL.shadowAttenuation;

                    addDiffuse += texColor.rgb * addCol * NdotLa;
                    addInt     += NdotLa * Luminance(addCol);
                }
                #endif

                // Total intensity drives fade + glow
                float totalInt = saturate(mainInt + addInt);

                float alpha = max(_MinAlpha, saturate(totalInt * _FadeStrength));
                float glow  = saturate(totalInt * _GlowStrength);
                float3 emission = _EmissionColor.rgb * (_EmissionIntensity * glow);

                float3 mainDiffuse = texColor.rgb * mainCol * NdotLm;
                float3 finalRGB    = mainDiffuse + addDiffuse + emission;

                return float4(finalRGB, texColor.a * alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
