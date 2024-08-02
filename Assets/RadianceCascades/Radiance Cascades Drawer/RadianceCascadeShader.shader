Shader "Unlit/RadianceCascadeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float2 _Resolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f input) : SV_Target
            {
                //In here we just loop over the four pixels of the cascade0 and average their value

                float2 pixelIndex = input.uv * _Resolution;

                int2 cascadeIndex = floor(pixelIndex / 2);//the dimension of cascade0 is 2, that's why we divide by that
                int2 cascadeOriginPosition = cascadeIndex * 2;

                float4 radiance = 0;
                for (int i = 0; i < 2; ++i) {
                    for (int j = 0; j < 2; ++j) {
                        float2 sampleIndex = cascadeOriginPosition + float2(i, j);
                        radiance += tex2D(_MainTex, sampleIndex / _Resolution);
                    }
                }

                radiance *= 0.25;

                return float4(radiance.rgb, 1);
            }
            ENDCG
        }
    }
}
