// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/EdgesBlend" {
    
    Properties{
        [HideInInspector]_Color("Color", Color) = (0,0,0,1)
        [HideInInspector]_Smooth("Smooth", Float) = 8
        [HideInInspector]_Strength("Strength", Float) = 1
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
            uniform fixed4 _Color;
            uniform float _Strength;
            uniform float _Smooth;
            
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

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

            half4 frag(vertexOutput i) : SV_Target
            {
                float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x);
                float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).y);

                fixed4 main = tex2D(_MainTex, i.uv);
                half charcoal = 0;

                for(int x = 1; x<=_Strength; x++)
                {
                    //half xP = _MainTex_TexelSize.x * 2;
                //half yP = _MainTex_TexelSize.y * 2;

                fixed3 tex0 = tex2D(_MainTex, i.uv + float2(stepX*x, 0)).rgb;
                fixed3 tex1 = tex2D(_MainTex, i.uv + float2(0, stepY*x)).rgb;

                charcoal += abs(main.r - tex0.r);
                charcoal += abs(main.g - tex0.g);
                charcoal += abs(main.b - tex0.b);
                charcoal += abs(main.r - tex1.r);
                charcoal += abs(main.g - tex1.g);
                charcoal += abs(main.b - tex1.b);

                tex0 = tex2D(_MainTex, i.uv - float2(stepX*x, 0)).rgb;
                tex1 = tex2D(_MainTex, i.uv - float2(0, stepY*x)).rgb;
                charcoal += abs(main.r - tex0.r);
                charcoal += abs(main.g - tex0.g);
                charcoal += abs(main.b - tex0.b);
                charcoal += abs(main.r - tex1.r);
                charcoal += abs(main.g - tex1.g);
                charcoal += abs(main.b - tex1.b);

                tex0 = tex2D(_MainTex, i.uv + float2(stepX*x, stepY*x)).rgb;
                tex1 = tex2D(_MainTex, i.uv + float2(stepX*x, -stepY*x)).rgb;
                charcoal += abs(main.r - tex0.r);
                charcoal += abs(main.g - tex0.g);
                charcoal += abs(main.b - tex0.b);
                charcoal += abs(main.r - tex1.r);
                charcoal += abs(main.g - tex1.g);
                charcoal += abs(main.b - tex1.b);

                tex0 = tex2D(_MainTex, i.uv + float2(-stepX*x, stepY*x)).rgb;
                tex1 = tex2D(_MainTex, i.uv + float2(-stepX*x, -stepY*x)).rgb;
                charcoal += abs(main.r - tex0.r);
                charcoal += abs(main.g - tex0.g);
                charcoal += abs(main.b - tex0.b);
                charcoal += abs(main.r - tex1.r);
                charcoal += abs(main.g - tex1.g);
                charcoal += abs(main.b - tex1.b);
                }

                charcoal = clamp(charcoal/_Smooth,0,1);

                //charcoal -= (1/_Strength);
                //charcoal = saturate(charcoal);
                return (1 - charcoal)*main + _Color * charcoal;
            }
            ENDCG
        }
    }
}