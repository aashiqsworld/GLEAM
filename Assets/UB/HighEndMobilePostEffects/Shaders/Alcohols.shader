// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Alcohols" {
    Properties{
        [HideInInspector]_Size("Size", Float) = 0.02
        [HideInInspector]_Main("Main", Float) = 1
        [HideInInspector]_Red("Red", Float) = .3
        [HideInInspector]_Green("Green", Float) = .3
        [HideInInspector]_Blue("Blue", Float) = .3
        [HideInInspector]_Seperate("Seperate channels", Float) = 1
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
            float _Size;
            float _Main;
            float _Red;
            float _Green;
            float _Blue;
            float _Seperate;
            
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

            half4 frag(vertexOutput i) : SV_Target
            {
                float3 color = tex2D(_MainTex, i.uv).rgb;
                if (_Seperate == 1)
                {
                    float r = tex2D(_MainTex, i.uv + float2(sin(_Time.y*2.0)*_Size, cos(_Time.y)*_Size)).r;
                    float g = tex2D(_MainTex, i.uv + float2(sin(_Time.y*3.0)*_Size, cos(_Time.y * 2)*_Size)).g;
                    float b = tex2D(_MainTex, i.uv + float2(sin(_Time.y)*_Size, cos(_Time.y * 3)*_Size)).b;
                    color = color*_Main + float3(r, 0, 0)*_Red + float3(0, g, 0)*_Green + float3(0, 0, b)*_Blue;
                }
                else {
                    float4 r = tex2D(_MainTex, i.uv + float2(sin(_Time.y*2.0)*_Size, cos(_Time.y)*_Size));
                    float4 g = tex2D(_MainTex, i.uv + float2(sin(_Time.y*3.0)*_Size, cos(_Time.y * 2)*_Size));
                    float4 b = tex2D(_MainTex, i.uv + float2(sin(_Time.y)*_Size, cos(_Time.y * 3)*_Size));
                    color = color*_Main + r*_Red + g*_Green + b*_Blue;
                }
                

                return float4(color, 1);
            }
            ENDCG
        }
    }
}