
Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size (px)", Float) = 4
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                if (col.a < 0.1)
                {
                    float2 pixelUV = fwidth(i.uv);
                    int size = (int)ceil(_OutlineSize);

                    [loop]
                    for (int x = -size; x <= size; x++)
                    {
                        [loop]
                        for (int y = -size; y <= size; y++)
                        {
                            if (x * x + y * y > _OutlineSize * _OutlineSize) continue;

                            if (tex2D(_MainTex, i.uv + float2(x, y) * pixelUV).a > 0.1)
                                return _OutlineColor;
                        }
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}
