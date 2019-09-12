// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB_Old/InvisiblesDirectionalDesktopOld" {
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			//Name "ShadowPass"
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(3,4)
			};

			float4 _Color;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float atten = LIGHT_ATTENUATION(i);
				return (1 - atten)*_Color;
			}
			ENDCG
		}
	}
	FallBack "Transparent/Cutout/VertexLit"
}