// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/CRTs" {
    Properties {
		[HideInInspector]_NoiseX("NoiseX", Range(0, 1)) = 0
		[HideInInspector]_Offset("Offset", Vector) = (0, 0, 0, 0)
		[HideInInspector]_RGBNoise("RGBNoise", Range(0, 1)) = 0
		[HideInInspector]_ScanLineTail("Tail", Float) = 0.5
		[HideInInspector]_ScanLineSpeed("TailSpeed", Float) = 100

		[HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}

    }
   
	Subshader {

		Pass {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
			//Tags{ "Queue" = "Opaque" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Degree;
			float _NoiseX;
			float2 _Offset;
			float _RGBNoise;
			float _ScanLineTail;
			float _ScanLineSpeed;
			
			float rand(float2 co) {
				return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
			}

			float2 mod(float2 a, float2 b)
			{
				return a - floor(a / b) * b;
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			struct vertexOutput {
				float4 vertex : POSITION;
				float2 screen_uv : TEXCOORD0;
			};

			vertexOutput vert(appdata v)
			{
				vertexOutput o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.screen_uv = TRANSFORM_TEX(v.uv, _MainTex); //v.uv;
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				float2 inUV = i.screen_uv; 
				float2 uv = i.screen_uv - 0.5; 

				float2 texUV = uv + 0.5;

				if (max(abs(uv.y) - 0.5, abs(uv.x) - 0.5) > 0)
				{
					return float4(0, 0, 0, 1);
				}

				float3 col;

				texUV += _Offset;
				texUV.x += (rand(floor(texUV.y * 500) + _Time.y) - 0.5) * _NoiseX;
				texUV = mod(texUV, 1);

				col.r = tex2D(_MainTex, texUV).r;
				col.g = tex2D(_MainTex, texUV - float2(0.002, 0)).g;
				col.b = tex2D(_MainTex, texUV - float2(0.004, 0)).b;

				if (rand((rand(floor(texUV.y * 500) + _Time.y) - 0.5) + _Time.y) < _RGBNoise)
				{
					col.r = rand(uv + float2(123 + _Time.y, 0));
					col.g = rand(uv + float2(123 + _Time.y, 1));
					col.b = rand(uv + float2(123 + _Time.y, 2));
				}

				float scanLineColor = sin(_Time.y * 10 + uv.y * 500) / 2 + 0.5;
				col *= 0.5 + clamp(scanLineColor + 0.5, 0, 1) * 0.5;

				float tail = clamp((frac(uv.y + _Time.y * _ScanLineSpeed) - 1 + _ScanLineTail) / min(_ScanLineTail, 1), 0, 1);
				col *= tail;

				return float4(col, 1);
			}
			ENDCG
		}
	}
}

