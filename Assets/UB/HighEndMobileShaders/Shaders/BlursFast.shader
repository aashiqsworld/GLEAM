// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/BlursFast"
{
	Properties
	{
		_CustomFloatParam1("Loop Count", float) = 9
		_CustomFloatParam2("Jump N Pixels", float) = 1
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	SubShader
	{
		Pass
		{
			ZTest Always 
			Cull Off
			Fog{ Mode off }

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
			float _CustomFloatParam1;
			float _CustomFloatParam2;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float2 Circle(float mStart, float mPoints, float mPoint) 
			{
				float Rad = (3.141592 * 2.0 * (1.0 / mPoints)) * (mPoint + mStart);
				return float2(sin(Rad), cos(Rad));
			}

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

			fixed4 frag(v2f i) : COLOR
			{
				float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x);
				float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).y);

				half4 color = float4 (0,0,0,0);

				fixed2 copyUV;

				for(int x = 1; x<=_CustomFloatParam1; x++)
				{
					copyUV = i.uv + Circle(0, _CustomFloatParam1, x)*float2(stepX,stepY)*_CustomFloatParam2;
					color += tex2D (_MainTex, copyUV)/_CustomFloatParam1;

				}

				return color;
			}

			ENDCG
		}
	}
}