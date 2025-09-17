 Shader "Custom/MaskedShader"
  {
      Properties
      {
          _MainTex ("Texture", 2D) = "white" {}
          _Mask ("Mask", 2D) = "white" {}
          _Color ("Tint Color", Color) = (1,1,1,1)
      }
      SubShader
      {
          Tags { "Queue"="Transparent" "RenderType"="Transparent" }
          LOD 100
          Blend SrcAlpha OneMinusSrcAlpha
          ZWrite Off

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
              sampler2D _Mask;
              float4 _MainTex_ST;

              v2f vert (appdata v)
              {
                  v2f o;
                  o.vertex = UnityObjectToClipPos(v.vertex);
                  o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                  return o;
              }

              fixed4 frag (v2f i) : SV_Target
              {
                  fixed4 col = tex2D(_MainTex, i.uv);
                  fixed4 mask = tex2D(_Mask, i.uv);

                  // 마스크의 알파 값을 사용해서 원본 텍스처의 알파를 조절
                  col.a *= mask.a;

                  return col;
              }
              ENDCG
          }
      }
  }