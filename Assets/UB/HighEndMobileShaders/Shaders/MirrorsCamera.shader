// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB_Old/MirrorsCamera"
{
	Properties
	{
		[HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
	}
		SubShader
		{
			Tags{ "RenderType" = "Opaque" }
			LOD 100

		Pass
			{
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
            
            uniform sampler2D _ReflectionTex;
            uniform float4 _ReflectionTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _ReflectionTex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return tex2D(_ReflectionTex, i.uv);
			}
			ENDCG
		}
	}
}