// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Posterizes" {
    Properties{
        [HideInInspector]_Level("Level", Float) = 8
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
            float _Level;

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
                float4 tex = tex2D(_MainTex, i.uv);

                float4 result = floor(tex * float4(_Level,_Level,_Level,_Level)) / 
                    float4(_Level,_Level,_Level,_Level);

                return result; 
            }
            ENDCG
        }
    }
}