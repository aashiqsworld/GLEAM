// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/MirrorsAlphaReflect"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_ReflAmount("Reflection amount", float) = 0.5
        _ReflDistort("Reflection distort", float) = 0.25
		[HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
	}
	SubShader
	{
	    Tags{
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }
		LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			sampler2D _ReflectionTex;
			float4 _Color;
			float _ReflAmount;
			float _ReflDistort;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _BumpMap);
				return o;
			}

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed3 nor = UnpackNormal(tex2D(_BumpMap, i.uv1));
			
				fixed2 screenUV = (i.screenPos.xy) / (i.screenPos.w+FLT_MIN);

				screenUV.xy += nor *_ReflDistort;
				fixed4 refl = tex2D(_ReflectionTex, screenUV);
				fixed4 color = (refl*_ReflAmount) + (tex*_Color*tex.a);
                color.a = _ReflAmount;
                return color;
			}
			ENDCG
		}
	}
}