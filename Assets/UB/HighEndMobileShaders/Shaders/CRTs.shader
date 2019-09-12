// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/CRTs" {
    Properties {
		_Degree("Degree", float) = 0
		_Darkness("Darkness",  Range(0, 1)) = 0.3
		_NoiseX("NoiseX", Range(0, 1)) = 0
		_Offset("Offset", Vector) = (0, 0, 0, 0)
		_RGBNoise("RGBNoise", Range(0, 1)) = 0
		_ScanLineTail("Tail", Float) = 0.5
		_ScanLineSpeed("TailSpeed", Float) = 100
		[HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }
    }
   
	Subshader {

		Pass {

			Tags{ "Queue" = "Opaque" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _ReflectionTex;
			float _Degree;
			float _NoiseX;
			float _Darkness;
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
				v.uv.xy -= 0.5;

				const float PI = 3.14159;
				float s = sin(_Degree*PI/180); //radian
				float c = cos(_Degree*PI/180);

				float2x2 rotationMatrix = float2x2(c, -s, s, c);
				rotationMatrix *= 0.5;
				rotationMatrix += 0.5;
				rotationMatrix = rotationMatrix * 2 - 1;
				v.uv.xy = mul(v.uv.xy, rotationMatrix);
				v.uv.xy += 0.5;
				
				o.screen_uv = v.uv;
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

				col.r = tex2D(_ReflectionTex, texUV).r;
				col.g = tex2D(_ReflectionTex, texUV - float2(0.002, 0)).g;
				col.b = tex2D(_ReflectionTex, texUV - float2(0.004, 0)).b;

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

				col *= 1 - _Darkness;

				return float4(col, 1);
			}
			ENDCG
		}
	}
}

