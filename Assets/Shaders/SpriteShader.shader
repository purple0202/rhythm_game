
  Shader "Custom/SpriteOutline"
  {
      Properties
      {
          _MainTex ("Sprite Texture", 2D) = "white" {}
          _OutlineColor ("Outline Color", Color) = (1,1,1,1)
          _OutlineSize ("Outline Size", Float) = 1
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
              float4 _MainTex_TexelSize;
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
                      float2 up    = i.uv + float2(0, _MainTex_TexelSize.y) * _OutlineSize;
                      float2 down  = i.uv - float2(0, _MainTex_TexelSize.y) * _OutlineSize;
                      float2 right = i.uv + float2(_MainTex_TexelSize.x, 0) * _OutlineSize;
                      float2 left  = i.uv - float2(_MainTex_TexelSize.x, 0) * _OutlineSize;

                      if (tex2D(_MainTex, up).a   > 0.1 ||
                          tex2D(_MainTex, down).a  > 0.1 ||
                          tex2D(_MainTex, right).a > 0.1 ||
                          tex2D(_MainTex, left).a  > 0.1)
                      {
                          return _OutlineColor;
                      }
                  }

                  return col;
              }
              ENDCG
          }
      }
  }