// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Glitchs" {
    Properties {
		[HideInInspector]_Noise("Noise", Float) = 0.04
		[HideInInspector]_Speed("Speed", Float) = 12
		[HideInInspector]_Size("Size", Float) = 3
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
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
			float _Noise;
			float _Speed;
			float _Size;

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

			float rng2(float2 seed)
			{
			    return frac(sin(dot(seed * floor(_Time.y * _Speed), float2(127.1,311.7))) * 43758.5453123);
			}

			float rng(float seed)
			{
			    return rng2(float2(seed, 1.0));
			}

			half4 frag(vertexOutput IN) : SV_Target
			{
			    float2 blockS = floor(IN.uv * float2(24., 9.));
			    float2 blockL = floor(IN.uv * float2(8., 4.));
			    
			    float r = rng2(IN.uv);
			    float3 noise = (float3(r, 1. - r, r / 2. + 0.5) * 1.0 - 2.0) * _Noise;
			    
			    float lineNoise = pow(rng2(blockS), 8.0) * pow(rng2(blockL), 3.0) - pow(rng(7.2341), 17.0) * _Size;
			    
			    float4 col1 = tex2D(_MainTex, IN.uv);
			    float4 col2 = tex2D(_MainTex, IN.uv + float2(lineNoise * 0.05 * rng(5.0), 0));
			    float4 col3 = tex2D(_MainTex, IN.uv - float2(lineNoise * 0.05 * rng(31.0), 0));
			    
				return float4(float3(col1.x, col2.y, col3.z) + noise, 1.0);
			}
			ENDCG
		}
	}
}