// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/ColorsChannel" {
    Properties{
        [HideInInspector]_Red("Red", Float) = 1
        [HideInInspector]_Green("Green",Float) = 1
        [HideInInspector]_Blue("Blue", Float) = 1
        [HideInInspector]_Luminance("Luminance", Float) = 1
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
            float _Red;
            float _Green;
            float _Blue;
            float _Luminance;
            
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
                float r = color.r*_Red;
                float g = color.g*_Green;
                float b = color.b*_Blue;

                return float4(r,g,b, 1)*_Luminance;
            }
            ENDCG
        }
    }
}
