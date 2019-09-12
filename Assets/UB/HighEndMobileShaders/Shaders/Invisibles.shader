// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/Invisibles" {
    Properties { 
         //_MainTex("Base (RGB)", 2D) = "white" {} 
     } 
 
     SubShader {     
         
         Pass { 
             Blend One One
             Tags { "LightMode" = "ForwardAdd"} 
             CGPROGRAM 
             #pragma vertex vert 
             #pragma fragment frag 
             #pragma multi_compile_fwdadd_fullshadows 
             #include "UnityCG.cginc"
             #include "Lighting.cginc"
             #include "AutoLight.cginc"
             //sampler2D _MainTex;
             //float4 _MainTex_ST;

             struct v2f { 
                 float4 pos : SV_POSITION; 
                 float3 worldPos : TEXCOORD0;
                 SHADOW_COORDS(1) //put to TEXCOORD1
             }; 
 
             v2f vert(appdata_base v) {  
                 v2f o;
                 o.pos = UnityObjectToClipPos (v.vertex);
                 o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                 TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
                 return o;
             } 
 
             float4 frag(v2f i) : COLOR
             { 
                 UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos) //atten is builtIn :)
                 //float4 tex = tex2D (_MainTex, i.uv);
                 float4 color = atten*_LightColor0; //lightColor comes from "Lighting.cginc"
                 return color;
             } 
             ENDCG 
         }

    }
     Fallback "VertexLit" 
 }
