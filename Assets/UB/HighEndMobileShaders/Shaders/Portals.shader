// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB_Old/Portals" {
    Properties {
		_WaterSpeed ("Water Speed", Float) = 0.2
		_ReflectionDistortion ("Reflection Distortion", Float) = 1
		_FakeDepth ("FakeDepth", Float) = 0.05
		_Emission ("Emission", Float) = 0.3
		_Alpha ("Alpha", Float) = 0.5
		_MainTex ("WaveTexture", 2D) = "white" {}
		_DepthTex ("DepthTexture", 2D) = "white" {}
		[HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }
    }

    SubShader {

    	Tags { 
			"Queue"="Transparent"  
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
		}
		LOD 100

		CGPROGRAM

		#pragma surface surf Lambert alpha vertex:vert

		#include "UnityCG.cginc"

		struct Input {
			float2 uv_MainTex;
			float2 uv_DepthTex;
			float4 vertexPos;
		};

		void vert(inout appdata_full v, out Input OUT)
		{
			UNITY_INITIALIZE_OUTPUT(Input, OUT);

			float4 clipSpacePosition = UnityObjectToClipPos(v.vertex);
			OUT.vertexPos = clipSpacePosition;
		}

		sampler2D _MainTex;
		sampler2D _DepthTex;

		uniform float _WaterSpeed;
		uniform float _ReflectionDistortion;
		uniform float _FakeDepth;
		uniform float _Emission;
		uniform float _Alpha;
        uniform sampler2D _ReflectionTex;

		void surf (Input IN, inout SurfaceOutput o) 
		{
			float2 screenUV = IN.vertexPos.xy / IN.vertexPos.w;

			screenUV = (screenUV + float2(1.0, 1.0)) * 0.5;

            float waveslide = _WaterSpeed*_Time.x;

			half3 col_orig = tex2D(_MainTex, float2(IN.uv_MainTex.x,IN.uv_MainTex.y+waveslide)).rgb*_FakeDepth;

			half3 col1 = tex2D(_DepthTex, float2(IN.uv_MainTex.x+col_orig.b,IN.uv_MainTex.y+col_orig.g)).rgb;
			fixed4 refl = tex2D(_ReflectionTex, float2(screenUV.x+col_orig.b*_ReflectionDistortion,screenUV.y+col_orig.g*_ReflectionDistortion));
			half3 norm1 = UnpackNormal (tex2D (_DepthTex, IN.uv_DepthTex));
		
			half3 col = col1+col1+float3(0,0,-0.07);
			o.Albedo = col;
			o.Alpha = _Alpha; 
		
			o.Emission = _Emission*refl;
			o.Normal = norm1;
		}

		ENDCG

	}
	Fallback "Diffuse"
}