Shader "Custom/ScorePopup"
{
    Properties
    {
        _MainTex   ("Font Texture", 2D)    = "white" {}
        _Color     ("Text Color",   Color) = (1,1,1,1)
        _GlowColor ("Glow Color",   Color) = (1,1,0,1)
        _GlowPower ("Glow Power",   Range(0, 3)) = 1.5
        _Alpha     ("Alpha",        Range(0, 1)) = 1.0
    }

    SubShader
    {
        // ✅ UI / Transparent queue — TMP ke saath kaam karta hai
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _Color;
            float4    _GlowColor;
            float     _GlowPower;
            float     _Alpha;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 col : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.col = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // Font texture sample
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Base color + vertex color
                fixed4 base = _Color * i.col;
                base.a *= tex.a * _Alpha;

                // Glow — text ke edges pe sona/rang
                fixed glow = pow(tex.a, _GlowPower);
                fixed4 glowCol = _GlowColor * glow;
                glowCol.a = glow * _Alpha;

                // Dono mix karo
                fixed4 final = base + glowCol * (1.0 - base.a);
                final.a = saturate(base.a + glowCol.a);

                return final;
            }
            ENDCG
        }
    }

    // TMP fallback
    Fallback "TextMeshPro/Mobile/Distance Field"
}
