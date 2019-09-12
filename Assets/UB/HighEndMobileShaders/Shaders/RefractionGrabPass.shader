Shader "UB_Old/RefractionGrabPass" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Refraction ("Refraction", Range (0.00, 100.0)) = 100.0
		_RefractionTex ("Refraction Text (RGB)", 2D) = "white" {}
		_BaseTex ("Base Text (RGB)", 2D) = "white" {}
	}

	SubShader {	

		Tags { 
		"Queue"="Transparent"  
		"IgnoreProjector"="True" 
		"RenderType"="Transparent" 
		}

		LOD 100

		GrabPass 
		{ 
			"_GrabTexture"
		}

		CGPROGRAM

		#pragma surface surf Lambert 

		fixed4 _Color;
		sampler2D _GrabTexture;
		sampler2D _RefractionTex;
		sampler2D _BaseTex;
		float _Refraction;

		float4 _GrabTexture_TexelSize;

		struct Input {
			float2 uv_RefractionTex;
			float2 uv_BaseTex;
			//float3 color;
			float3 worldRefl; 
			float4 screenPos;
			INTERNAL_DATA
		};

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 tex = tex2D (_BaseTex, IN.uv_BaseTex);
		    fixed3 nor = UnpackNormal (tex2D(_RefractionTex, IN.uv_RefractionTex));

		    //fixed
		    fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);
		    //screenUV.x = 1-screenUV.x; 
		    screenUV.xy += nor *_Refraction;
		   
		    fixed4 refl = tex2D (_GrabTexture, screenUV);
		   
		    o.Albedo = tex.rgb *_Color.rgb;
		    o.Normal = nor.rgb;
		    o.Emission = refl.rgb *tex.rgb;

		}
		ENDCG
	}

	FallBack "Diffuse"
}