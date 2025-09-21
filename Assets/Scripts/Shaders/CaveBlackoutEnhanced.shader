// Enhanced version with additional features
Shader "Custom/CaveBlackoutEnhanced"
{
    Properties
    {
        _Color ("Blackout Color", Color) = (0,0,0,1)
        _Intensity ("Blackout Intensity", Range(0, 1)) = 1.0
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 0.1)) = 0.01
        _EdgeFade ("Edge Fade", Range(0, 1)) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
        }
        
        LOD 200
        Cull Off
        ZWrite On
        ZTest LEqual
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_FOG_COORDS(1)
            };

            fixed4 _Color;
            float _Intensity;
            sampler2D _NoiseTexture;
            float4 _NoiseTexture_ST;
            float _NoiseStrength;
            float _EdgeFade;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTexture);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                
                // Add subtle noise for more natural darkness
                if(_NoiseStrength > 0)
                {
                    fixed4 noise = tex2D(_NoiseTexture, i.uv * 10.0);
                    col.rgb = lerp(col.rgb, col.rgb + (noise.rgb - 0.5) * _NoiseStrength, _NoiseStrength);
                }
                
                // Edge fade effect
                if(_EdgeFade > 0)
                {
                    float2 center = i.uv - 0.5;
                    float dist = length(center);
                    float fade = 1.0 - smoothstep(0.5 - _EdgeFade, 0.5, dist);
                    col.a *= fade;
                }
                
                col.rgb *= _Intensity;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}