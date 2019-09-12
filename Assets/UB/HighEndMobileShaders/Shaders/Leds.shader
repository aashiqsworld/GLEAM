// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB_Old/Leds"
{
    Properties
    {
        _ScreenWidth("Screen Width", float) = 300
        _ScreenHeight("Screen Height", float) = 150
        _LedSize("Led Size", float) = 2
        _Luminance("Luminance", float) = 1
        _ScanColor("Scan Color", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }
        //_MainTex("Base (RGB)", 2D) = "" {}
    }

    SubShader
    {
        Pass
        {
            Tags { "Queue" = "Opaque" }

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
                //float4 screen_pos : TEXCOORD1;
                //float4 worldPos : TEXCOORD2;
            };

            sampler2D _ReflectionTex;
            float4 _ReflectionTex_ST;
            float _ScreenWidth;
            float _ScreenHeight;
            float _LedSize;
            float _Luminance;
            float4 _ScanColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ReflectionTex);
                //o.screen_pos = ComputeScreenPos(o.vertex);
                //o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float mod(float a, float b)
            {
                return a - floor(a / b) * b;
            }
            float2 mod(float2 a, float2 b)
            {
                return a - floor(a / b) * b;
            }
            float3 mod(float3 a, float3 b)
            {
                return a - floor(a / b) * b;
            }
            float4 mod(float4 a, float4 b)
            {
                return a - floor(a / b) * b;
            }

            float4 frag(v2f i) : COLOR
            {
                float2 screenUV = i.uv; //i.screen_pos.xy / i.screen_pos.w; 
                //float pixelsX = _ScreenParams.x; //to find i.uv.x is which pixel on screen resolution :)
                //float pixelsY = _ScreenParams.y;

                float4 color = tex2D(_ReflectionTex, i.uv);

                //default gray
                float4 lcd = float4(0.4, 0.4, 0.4,1.0);



                int px = int(mod(screenUV.x*_ScreenWidth, _LedSize*3.0));
                if (px < _LedSize) lcd.r = 1.0;
                else if (px < _LedSize*2.0) lcd.g = 1.0;
                else lcd.b = 1.0;

                //scanline
                if (int(mod(screenUV.y*_ScreenHeight, _LedSize*3.0)) == 0) {
                    lcd.rgb = _ScanColor;
                }

                return color*lcd*_Luminance;

            }

            ENDCG
        }
    }
}