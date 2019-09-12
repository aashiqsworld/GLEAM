// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Leds"
{
    Properties
    {
        _LedSize("Led Size", float) = 2
        _Luminance("Luminance", float) = 1
        _ScanColor("Scan Color", Color) = (0.5, 0.5, 0.5, 1)
        _MainTex("Base (RGB)", 2D) = "" {}
    }

    SubShader
    {
        Pass
        {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags { "Queue" = "Opaque" }

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

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _LedSize;
            float _Luminance;
            float4 _ScanColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
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
                //float2 screenUV = i.screen_pos.xy / i.screen_pos.w; 
                float pixelsX = _ScreenParams.x; //to find i.uv.x is which pixel on screen resolution :)
                float pixelsY = _ScreenParams.y;

                float4 color = tex2D(_MainTex, i.uv);

                //default gray
                float4 lcd = float4(0.4, 0.4, 0.4,1.0);

                int px = int(mod(i.uv.x*pixelsX, _LedSize*3.0));
                if (px < _LedSize) lcd.r = 1.0;
                else if (px < _LedSize*2.0) lcd.g = 1.0;
                else lcd.b = 1.0;

                //scanline
                if (int(mod(i.uv.y*pixelsY, _LedSize*3.0)) == 0) {
                    lcd.rgb = _ScanColor;
                }

                return color*lcd*_Luminance;

            }

            ENDCG
        }
    }
}