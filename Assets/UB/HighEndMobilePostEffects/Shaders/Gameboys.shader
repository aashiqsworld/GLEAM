// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Gameboys" {
    Properties {
		[HideInInspector]_BlockSize("Block Size", Float) = 3.0
		[HideInInspector]_Color1("Color 1", Color) = (0.03137, 0.09803, 0.12549, 1)
		[HideInInspector]_Color2("Color 2", Color) = (0.19607, 0.41568, 0.30980, 1)
		[HideInInspector]_Color3("Color 3", Color) = (0.53725,0.75294, 0.43529, 1)
		[HideInInspector]_Color4("Color 4", Color) = (0.87450, 0.96470, 0.81568, 1)
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
    }
   
	Subshader{

		Pass{
			Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float _BlockSize;
			
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

			float mmod(float x, float y)
			{
				return x - y * floor(x / y);
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				float4 color;

				float2 size = (_ScreenParams.zw - 1.0) * _BlockSize;
				float2 uv = i.uv - fmod(i.uv, size) + size*0.5;

				int level = int(max(1.0, ceil(tex2D(_MainTex, uv).r * 7.)));
				float dither = mmod(floor(i.uv.y / size) + floor(i.uv.x / size), 2.);

				if (mmod(float(level), 2.) < 1.) {
					level += (1 - int(dither) * 2);
				}

				if (level <= 1) color = _Color1;
				else if (level <= 3) color = _Color2;
				else if (level <= 5) color = _Color3;
				else color = _Color4;

				return color;
			}
			ENDCG
		}
	}
}