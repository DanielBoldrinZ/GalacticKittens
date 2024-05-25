Shader "Sprites/Outline"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineSize("Outline Size", Float) = 1.0
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

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    o.color = v.color * _Color;
                    return o;
                }

                fixed4 SampleTextureWithOutline(float2 uv, float2 offset)
                {
                    return tex2D(_MainTex, uv + offset);
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

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
                            return _OutlineColor;
                        }
                    }
                }

                // Apply transparency to the inner image
                //col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
        }
}