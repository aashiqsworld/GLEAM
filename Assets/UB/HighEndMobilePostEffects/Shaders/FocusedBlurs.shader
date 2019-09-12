// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/FocusedBlurs" {

	Properties	{
		[HideInInspector]_Focus("Focus", Vector) = (0.5, 0.5, 1,1)
		[HideInInspector]_Step("Step", Float) = 10.0
		[HideInInspector]_Radius("Radius", Float) = 0.1
		[HideInInspector]_NeighbourPixels("NeighbourPixels", Float) = 10
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}
	}

	Subshader{

		Pass {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            Tags { "Queue" = "Opaque" }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float4 _Focus;
			float _Step;
			float _Radius;
			float _NeighbourPixels;
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

			half4 frag(vertexOutput IN) : SV_Target
			{
			    float angle = atan2(IN.uv.x - _Focus.x,IN.uv.y - _Focus.y);
			    float csv = cos(angle);
			    float snv = sin(angle);

			    float len = distance(IN.uv,float2(_Focus.x,_Focus.y));

			    len = len < _Radius ? 0.0 : len-_Radius;
			        
			    float4 color = float4(0.,0.,0.,0.);
			    for(float i = -_Step; i < _Step ; i++)
			    {
			        float dis = len*i*(_NeighbourPixels/_ScreenParams.x);
			    	float2 offsetUv = float2(csv,snv)*dis;
			    	color += tex2D(_MainTex,IN.uv+offsetUv);
			    }

			    return color/(_Step*2.0);
			}
			ENDCG
		}
	}
}



