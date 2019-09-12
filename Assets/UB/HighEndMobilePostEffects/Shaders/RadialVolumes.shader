Shader "UB/PostEffects/RadialVolumes" {
    Properties{
        [HideInInspector]_Size("Size", Float) = 45
        [HideInInspector]_Step("Step", Float) = 10
        [HideInInspector]_Smooth("Smooth", Float) = 0.08
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
            float _Step;
            float _Smooth;
            
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

            #define T tex2D(_MainTex,.5+(p*=(1.-_Step)))

            half4 frag(vertexOutput i) : SV_Target
            {
                float2 p = i.uv-.5;
                _Step = (1/_ScreenParams.x)*_Step;
                float3 o = T.rgb;
                float z = 0;
                for (float index=0.;index<_Size;index++) 
                    z += pow(max(0.,.5-length(T.rgb)),2)*exp(-index*_Smooth);
                return float4(o+z,1);
            }
            ENDCG
        }
    }
}