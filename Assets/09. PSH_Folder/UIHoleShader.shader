Shader "UI/HoleShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,0.5) // 바깥 영역의 색깔이에요. 반투명 검은색!

        // C# 스크립트에서 채워줄 값들이에요.
        _HoleCenter ("Hole Center", Vector) = (0,0,0,0)
        _HoleSize ("Hole Size", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _HoleCenter;
            float2 _HoleSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 현재 픽셀과 구멍 중심 사이의 거리를 계산해요.
                float dx = abs(i.texcoord.x - _HoleCenter.x);
                float dy = abs(i.texcoord.y - _HoleCenter.y);

                // 구멍의 절반 크기를 계산해요.
                float halfWidth = _HoleSize.x / 2.0;
                float halfHeight = _HoleSize.y / 2.0;

                // 만약 현재 픽셀이 구멍 안에 있다면?
                if (dx < halfWidth && dy < halfHeight)
                {
                    // clip(-1)은 이 픽셀을 아예 그리지 말라는 뜻이에요!
                    clip(-1);
                }

                // 구멍 바깥이라면 지정된 오버레이 색상을 칠해요.
                return _Color;
            }
            ENDCG
        }
    }
}