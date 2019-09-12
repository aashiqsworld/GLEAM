// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/Lego" {

	Properties	{
		[HideInInspector]_BlockSize("Block Size", Float) = 30
		[HideInInspector]_Shadow("Shadow", Range(0, 1)) = 1
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
			float _Shadow;
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
				float c = 1 / _BlockSize; // / _ScreenParams.xy;
			    float2 middle = floor(i.uv*_ScreenParams.xy*c+.5)/c;

			    float3 color = tex2D(_MainTex,middle/_ScreenParams.xy).rgb;
			    

		        float dis = distance(i.uv*_ScreenParams.xy,middle)*c*2.;    
		        if(dis<0.65 && dis>0.55){
		            color *= dot(float2(0.707,0.707),normalize(i.uv*_ScreenParams.xy-middle))*.5+1.;
		        }

		        if (_Shadow==1)
		        {
			        float2 delta = abs(i.uv*_ScreenParams.xy-middle)*c*2.;
			        float shadowDis = max(delta.x,delta.y);
			        if(shadowDis>.9){
			            color *= .8;
			        }
		        }
			    
				return float4(color,1.0);
			}
			ENDCG
		}
	}
}


