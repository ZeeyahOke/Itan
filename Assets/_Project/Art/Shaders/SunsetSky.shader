// Ìtàn evening sky: indigo overhead, rose in the middle, warm orange at the
// horizon, brighter around the setting sun. Cheap enough for Quest (no textures).
Shader "Itan/SunsetSky"
{
    Properties
    {
        _ZenithColor ("Zenith (overhead)", Color) = (0.13, 0.15, 0.38, 1)
        _MidColor ("Middle sky", Color) = (0.62, 0.42, 0.55, 1)
        _HorizonColor ("Horizon", Color) = (0.98, 0.58, 0.32, 1)
        _GroundColor ("Below horizon", Color) = (0.28, 0.18, 0.15, 1)
        _MidHeight ("Middle height", Range(0.02, 0.8)) = 0.22
        _SunColor ("Sun glow", Color) = (1, 0.72, 0.42, 1)
        _SunDiscSize ("Sun disc size", Range(0.995, 0.9999)) = 0.9993
        _SunGlow ("Sun glow spread", Range(1, 64)) = 10
        _SunGlowStrength ("Sun glow strength", Range(0, 3)) = 1.1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            float4 _ZenithColor, _MidColor, _HorizonColor, _GroundColor, _SunColor;
            float _MidHeight, _SunDiscSize, _SunGlow, _SunGlowStrength;
            float4 _ItanSunDirection; // set from C#: direction TOWARD the sun

            struct appdata { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float h = d.y;

                // Vertical gradient: horizon -> middle -> zenith, darker ground below.
                float3 sky = lerp(_HorizonColor.rgb, _MidColor.rgb, saturate(h / _MidHeight));
                sky = lerp(sky, _ZenithColor.rgb, saturate((h - _MidHeight) / (1 - _MidHeight)));
                float3 col = h >= 0 ? sky : lerp(_HorizonColor.rgb * 0.6, _GroundColor.rgb, saturate(-h * 6));

                // Sun: soft glow plus a small bright disc.
                float3 sunDir = normalize(_ItanSunDirection.xyz);
                float s = saturate(dot(d, sunDir));
                col += _SunColor.rgb * pow(s, _SunGlow) * _SunGlowStrength;
                col += _SunColor.rgb * smoothstep(_SunDiscSize, _SunDiscSize + 0.0004, s) * 2;
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
