// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Pulses"
{
    Properties
    {
        [HideInInspector]_Speed("Speed", Float) = 5
        [HideInInspector]_Iteration("Iteration", Float) = 10
        [HideInInspector]_Min("Min", Float) = .1
        [HideInInspector]_Max("Max", Float) = .3
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}
    }

    SubShader
    {
        Pass
        {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //ZTest Always 
            //Cull Off
            //Fog{ Mode off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float _Iteration;
            float _Min;
            float _Max;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                half4 color = half4(0, 0, 0, 0);

                float time = lerp(_Min, _Max, (sin(_Time.y*_Speed)+1)/2);

                float2 uv = i.uv;
                for (int i = 1; i <= _Iteration; i++)
                {
                    float step = float(i) / _Iteration;
                    color += tex2D(_MainTex, uv + (float2(0.5, 0.5) - uv)*step*time) / _Iteration;
                }

                return color;
            }
            ENDCG
        }
    }
}