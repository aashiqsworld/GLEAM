// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/HorizontalWaves" {
    Properties {
		[HideInInspector]_Speed("Speed", Float) = 10
		[HideInInspector]_SizeX("SizeX", Float) = 30
		[HideInInspector]_SizeY("SizeY", Float) = 10
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
    }
   
	Subshader{

		Pass{
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags{ "Queue" = "Opaque" }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _SizeX;
			float _SizeY;
			
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
    
			    float x = _ScreenParams.x*i.uv.x;
			    float y = _ScreenParams.y*i.uv.y;
			    
			    float z = floor((y/_SizeY) + 0.5);
			    
			    float x2 = x + (sin(z + (_Time.x * _Speed)) * _SizeX);
			    
			    float2 uv2 = float2(x2/_ScreenParams.x, y/_ScreenParams.y);
			    
			    return tex2D( _MainTex, uv2);
			}
			ENDCG
		}
	}
}