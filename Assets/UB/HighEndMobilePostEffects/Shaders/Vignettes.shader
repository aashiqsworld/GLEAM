// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Vignettes" {
    Properties{
        [HideInInspector]_Size("Size", Float) = 5.8
        [HideInInspector]_Blackness("Blackness", Float) = 0.32
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
            float _Blackness;
            
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
                fixed4 tex = tex2D(_MainTex, i.uv);
                float2 uv = i.uv - 0.5;
                fixed vig = 1.0 - dot(uv, uv);
                tex.rgb *= saturate(pow(vig, _Size) + _Blackness);
                return tex;
            }
            ENDCG
        }
    }
}