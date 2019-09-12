// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Stitchings" {
    Properties{
        [HideInInspector]_Invert("Invert", Float) = 1
        [HideInInspector]_Size("Size", Float) = 8
        [HideInInspector]_Color("Color", Color) = (0.0, 0.0, 0.0, 1)
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
    }

    Subshader{

        Pass{
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags{ "Queue" = "Opaque" }

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Invert;
            float _Size;
            float4 _Color;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            vertexOutput vert(appdata v)
            {
                vertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float mod(float a, float b)
            {
                return a - floor(a / b) * b;
            }
            float2 mod(float2 a, float2 b)
            {
                return a - floor(a / b) * b;
            }
            float3 mod(float3 a, float3 b)
            {
                return a - floor(a / b) * b;
            }
            float4 mod(float4 a, float4 b)
            {
                return a - floor(a / b) * b;
            } 

            half4 frag(vertexOutput i) : SV_Target
            {
                float4 color = float4(0,0,0,0);
                float2 colorPos = i.uv * _ScreenParams.xy;
                float2 tlPos = floor(colorPos / float2(_Size, _Size));
                tlPos *= _Size;
                int remX = int(mod(colorPos.x, _Size));
                int remY = int(mod(colorPos.y, _Size));
                if (remX == 0 && remY == 0)
                    tlPos = colorPos;
                float2 blPos = tlPos;
                blPos.y += (_Size - 1.0);
                if ((remX == remY) || (((int(colorPos.x) - int(blPos.x)) == (int(blPos.y) - int(colorPos.y)))))
                {
                    if (_Invert == 1)
                        color = _Color;
                    else
                        color = tex2D(_MainTex, tlPos * float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y));
                }
                else
                {
                    if (_Invert == 1)
                        color = tex2D(_MainTex, tlPos * float2(1.0/_ScreenParams.x, 1.0/_ScreenParams.y));
                    else
                        color = _Color;
                }
                return color;
            }
            ENDCG
        }
    }
}