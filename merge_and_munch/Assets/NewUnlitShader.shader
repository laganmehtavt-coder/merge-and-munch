Shader "Unlit/BlobJelly_Dynamic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ContactPoint ("Contact Point (Local)", Vector) = (0,0,0,0)
        _SqueezeAmount ("Squeeze Intensity", Range(0, 1)) = 0
        _SqueezeRadius ("Effect Radius", Range(0, 5)) = 1.5
        _BulgePower ("Bulge (Side Stretch)", Range(0, 1)) = 0.5
        _WobbleSpeed ("Wobble Speed", Range(0, 100)) = 40
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _ContactPoint;
            float _SqueezeAmount;
            float _SqueezeRadius;
            float _BulgePower;
            float _WobbleSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                float4 pos = v.vertex;

                // 1. Local Mask based on collision point
                float dist = distance(pos.xy, _ContactPoint.xy);
                float mask = saturate(1.0 - (dist / _SqueezeRadius));
                float effect = pow(mask, 2) * _SqueezeAmount;

                // 2. THE BLOB LOGIC (Squash and Stretch)
                // Niche se dabao (Y compression)
                pos.y -= effect * 0.6;
                
                // Side se phoolna (X expansion) - Yehi wo "Blob Drop" wala feel hai
                // Hum X ko badhate hain based on how much Y is squeezed
                float sideStretch = effect * _BulgePower;
                pos.x += (pos.x > 0 ? 1 : -1) * sideStretch * (1.0 - abs(pos.y));

                // 3. JIGGLE (Wobble after impact)
                float wobble = sin(_Time.y * _WobbleSpeed + v.uv.y * 5.0);
                pos.x += wobble * (_SqueezeAmount * 0.05) * mask;

                o.vertex = UnityObjectToClipPos(pos);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}