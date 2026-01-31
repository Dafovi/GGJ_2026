Shader "UI/ProximityOverlayV2"
{
    Properties
    {
        _Darkness("Darkness", Range(0,1)) = 0.92
        _Left("Left", Range(0,1)) = 0
        _Right("Right", Range(0,1)) = 0
        _Front("Front", Range(0,1)) = 0
        _Back("Back", Range(0,1)) = 0

        _EdgeWidthX("EdgeWidthX", Range(0.01,0.8)) = 0.20
        _EdgeWidthY("EdgeWidthY", Range(0.01,0.8)) = 0.30

        _SideStrength("SideStrength", Range(0,2)) = 1.0
        _TopStrength("TopStrength", Range(0,2)) = 1.15
        _BottomStrength("BottomStrength", Range(0,2)) = 1.55

        _Noise("Noise", Range(0,2)) = 0.7
        _TimeScale("TimeScale", Range(0,10)) = 3

        _GlowColor("GlowColor", Color) = (1,1,1,1)

        _HitAmount("HitAmount", Range(0,1)) = 0
        _HitColor("HitColor", Color) = (1,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Darkness;
            float _Left, _Right, _Front, _Back;

            float _EdgeWidthX, _EdgeWidthY;
            float _SideStrength, _TopStrength, _BottomStrength;

            float _Noise, _TimeScale;

            float4 _GlowColor;

            float _HitAmount;
            float4 _HitColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float vign = smoothstep(0.95, 0.22, length(uv - 0.5));
                float baseA = saturate(_Darkness * lerp(0.6, 1.0, vign));

                float leftEdge = smoothstep(_EdgeWidthX, 0.0, uv.x);
                float rightEdge = smoothstep(_EdgeWidthX, 0.0, 1.0 - uv.x);

                float topEdge = smoothstep(_EdgeWidthY, 0.0, 1.0 - uv.y);
                float bottomEdge = smoothstep(_EdgeWidthY, 0.0, uv.y);

                float t = _Time.y * _TimeScale;
                float n = (hash21(uv * 220 + t) - 0.5) * _Noise;

                float side = (leftEdge * _Left + rightEdge * _Right) * _SideStrength;
                float top = topEdge * _Front * _TopStrength;
                float bottom = bottomEdge * _Back * _BottomStrength;

                float glow = saturate(side + top + bottom);
                glow = saturate(glow + n * glow);

                float3 glowRGB = _GlowColor.rgb * glow;

                float hit = saturate(_HitAmount);
                float3 hitRGB = _HitColor.rgb * hit;

                float3 rgb = glowRGB + hitRGB;

                float a = saturate(baseA + glow * 0.55 + hit * 0.65);

                return float4(rgb, a);
            }
            ENDCG
        }
    }
}   