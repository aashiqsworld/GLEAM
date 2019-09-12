// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB_Old/InvisiblesDirectionalOld" {
    Properties { 
        [Toggle] _EnableColor("Enable Directional Color?", float) = 0
         //_MainTex("Base (RGB)", 2D) = "white" {} 
     } 
  
    SubShader
    {
        Pass { 
            Tags {"Queue"="Alpha"
                "RenderType"="Transparent"}
             Blend Zero SrcColor// One
             Tags { "LightMode" = "ForwardBase"} 
             CGPROGRAM 
             #pragma vertex vert 
             #pragma fragment frag 
             #pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight
             #include "UnityCG.cginc"
             #include "Lighting.cginc"
             #include "AutoLight.cginc"
             uniform float _EnableColor;
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
                 //do not use light color, it may lighten the gorund :)
                 float4 color = atten; //*_LightColor0; //lightColor comes from "Lighting.cginc"
                 if(_EnableColor)
                 {
                    color = atten*_LightColor0; 
                 }
                 //color.a = 1;
                 return color;

             } 
             ENDCG 
         }
        
     }
     Fallback "Transparent/Cutout/VertexLit"
}