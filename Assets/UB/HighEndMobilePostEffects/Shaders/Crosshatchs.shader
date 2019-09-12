// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Crosshatchs" {
    Properties{
        [HideInInspector]_Color1("Color1", Color) = (0, 0, 0, 1)
        [HideInInspector]_Color2("Color2", Color) = (1, 1, 1, 1)
        [HideInInspector]_Size("Size", int) = 5
        [HideInInspector]_Detail("Detail", int) = 1
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
            int _Size;
            int _Detail;
            float4 _Color1;
            float4 _Color2;

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

            //const 
            //const 

            half4 frag(vertexOutput i) : SV_Target
            {
                int x = _ScreenParams.x*i.uv.x;
                int y = _ScreenParams.y*i.uv.y;

                float lum = length(tex2D(_MainTex, i.uv).rgb);
                //float4 tex = float4(0,0,0,0);

                //int _Size = 5;
                //int _Detail = 1;
                if (lum < 1.00) 
                {
                    if (mod(x + y, _Size) == 0.0) 
                    {
                        return _Color1;
                    }
                }

                if (lum < 0.75) 
                {
                    if (mod(x - y, _Size) == 0.0) 
                    {
                        return _Color1;
                    }
                }

                if (lum < 0.50) 
                {
                    if (mod(x + y - _Detail, _Size) == 0.0) 
                    {
                        return _Color1;
                    }
                }

                if (lum < 0.25) 
                {
                    if (mod(x - y - _Detail, _Size) == 0.0) 
                    {
                        return _Color1;
                    }
                }

                return _Color2;
            }
            ENDCG
        }
    }
}