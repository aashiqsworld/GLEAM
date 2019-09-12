// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/SimplesNoise"
{
	Properties
	{
		[HideInInspector]_Noise("Noise", float) = 50
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
			float _Noise;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float mod(float a, float b)
			{
				return a - floor(a / b) * b;
			}
			float2 mod(float2 a, float2 b)
			{
				return a - floor(a / b) * b;
			}
			float3 mod(float3 a, float3 b)
			{
				return a - floor(a / b) * b;
			}
			float4 mod(float4 a, float4 b)
			{
				return a - floor(a / b) * b;
			} 

			fixed4 frag(v2f i) : COLOR
			{
			    float4 color = tex2D (_MainTex, i.uv);

			    float x = (i.uv.x + 4 ) * (i.uv.y + 4 ) * _Time.y * 10;
			    float grain = mod((mod(x, 13.0) + 1.0) * (mod(x, 123.0) + 1.0), 0.01)-0.005;
				float4 noise = float4(grain,grain,grain,grain) * _Noise;
			    
			    return color + noise;
			}

			ENDCG
		}
	}
}

