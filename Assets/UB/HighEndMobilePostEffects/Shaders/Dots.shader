// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/Dots" {

	Properties	{
		[HideInInspector]_BlockSize("Block Size", Float) = 15.0
		[HideInInspector]_DotRadius("Dot Radius", Float) = 5.0
		[HideInInspector]_Color("Empty Color", Color) = (0.0, 0.0, 0.0, 1)
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}
	}

	Subshader{

		Pass {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags { "Queue" = "Opaque" }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float _BlockSize;
			float _DotRadius;
			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

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
				float2 pixel = floor((i.uv*_ScreenParams.xy)/_BlockSize)*_BlockSize;
				float2 center = (pixel + _BlockSize / 2.0);
				float distance = length(center - i.uv*_ScreenParams.xy);

				if (distance > _DotRadius)
				{
					return _Color;
				}
				else
				{
					return tex2D(_MainTex, i.uv);;
				}
			}
			ENDCG
		}
	}
}


