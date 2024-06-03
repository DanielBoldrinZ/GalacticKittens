Shader "Custom/PerlinNoiseSpriteShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 5.0
        _Speed("Speed", Float) = 1.0
        _Opacity("Opacity", Range(0, 1)) = 1.0
        _Contrast("Contrast", Float) = 1.0
        _MovementSpeed("Movement Speed", Float) = 1.0
        _Brightness("Brightness", Range(0, 1)) = 1.0 // New property for brightness adjustment
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
            LOD 200

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _NoiseScale;
                float _Speed;
                float _Opacity;
                float _Contrast;
                float _MovementSpeed;
                float _Brightness; // New variable for brightness adjustment
                float4x4 _CamToWorld;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    float4 worldPos = mul(_CamToWorld, v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex) + worldPos.xy * _NoiseScale * _MovementSpeed;
                    return o;
                }

                float noise(float2 p)
                {
                    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
                }

                float smoothNoise(float2 p)
                {
                    float2 pi = floor(p);
                    float2 pf = frac(p);

                    float f00 = noise(pi);
                    float f10 = noise(pi + float2(1.0, 0.0));
                    float f01 = noise(pi + float2(0.0, 1.0));
                    float f11 = noise(pi + float2(1.0, 1.0));

                    float2 blend = pf * pf * (3.0 - 2.0 * pf);
                    return lerp(lerp(f00, f10, blend.x), lerp(f01, f11, blend.x), blend.y);
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv * _NoiseScale;
                    uv += _Time.y * _Speed * 0.1;
                    float n = smoothNoise(uv);
                    n = pow(n, _Contrast);
                    n *= _Brightness; // Adjust brightness
                    float4 noiseColor = fixed4(n, n, n, 1.0);
                    float4 mainTexColor = tex2D(_MainTex, i.uv);
                    return lerp(mainTexColor, noiseColor, _Opacity);
                }
                ENDCG
            }
        }
            FallBack "Transparent/VertexLit"
}