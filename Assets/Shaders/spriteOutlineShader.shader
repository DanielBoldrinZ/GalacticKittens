Shader "Sprites/OutlineWithCurvedChalkEffect"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineSize("Outline Size", Float) = 1.0
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.5
        _GrainScale("Grain Scale", Float) = 10.0
        _ChalkAngle("Chalk Angle", Range(0, 360)) = 45.0
        _Perturbation("Perturbation", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _GrainIntensity;
            float _GrainScale;
            float _ChalkAngle;
            float _Perturbation;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float Random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float2 RotateUV(float2 uv, float angle)
            {
                float rad = angle * 3.14159265 / 180.0;
                float cosA = cos(rad);
                float sinA = sin(rad);
                return float2(
                    uv.x * cosA - uv.y * sinA,
                    uv.x * sinA + uv.y * cosA
                );
            }

            float2 Perturb(float2 uv, float angle, float perturbation)
            {
                float2 offset = float2(cos(angle), sin(angle)) * perturbation;
                return uv + offset;
            }

            fixed4 SampleTextureWithOutline(float2 uv, float2 offset)
            {
                return tex2D(_MainTex, uv + offset);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                // Convert to grayscale
                float grayscale = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = float3(grayscale, grayscale, grayscale);

                bool isOutline = false;

                // If the current pixel is transparent, check surrounding pixels for outline
                if (col.a == 0)
                {
                    float2 offsets[8] = {
                        float2(_OutlineSize, 0),
                        float2(-_OutlineSize, 0),
                        float2(0, _OutlineSize),
                        float2(0, -_OutlineSize),
                        float2(_OutlineSize, _OutlineSize),
                        float2(-_OutlineSize, -_OutlineSize),
                        float2(_OutlineSize, -_OutlineSize),
                        float2(-_OutlineSize, _OutlineSize)
                    };

                    for (int j = 0; j < 8; j++)
                    {
                        fixed4 sample = SampleTextureWithOutline(i.texcoord, offsets[j] * _MainTex_TexelSize.xy);
                        if (sample.a > 0)
                        {
                            isOutline = true;
                            break;
                        }
                    }
                }

                if (isOutline)
                {
                    // Generate procedural grain with curved directional noise
                    float grain = 0.0;
                    float2 direction = RotateUV(float2(_GrainScale, 0), _ChalkAngle);
                    float perturbationAngle = _ChalkAngle;

                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            float2 offset = Perturb(float2(x, y) * direction * _MainTex_TexelSize.xy, perturbationAngle, _Perturbation);
                            grain += Random(i.texcoord + offset);
                            perturbationAngle += _Perturbation; // Gradually change angle
                        }
                    }
                    grain /= 9.0; // Average the grain values

                    // Apply grain effect to the outline
                    col = lerp(_OutlineColor, _OutlineColor * grain, _GrainIntensity);
                }

                return col;
            }
            ENDCG
        }
    }
}