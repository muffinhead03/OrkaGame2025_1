Shader "UI/GlitchEffect"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _GlitchTex("Glitch Texture", 2D) = "white" {}
        _GlitchAmount("Glitch Amount", Range(0,1)) = 1
        _GlitchCutAmountX("Cut X", Range(0.1, 10)) = 1
        _GlitchCutAmountY("Cut Y", Range(0.1, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _GlitchTex;
            float4 _MainTex_ST;
            float _GlitchAmount;
            float _GlitchCutAmountX;
            float _GlitchCutAmountY;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

fixed4 frag (v2f i) : SV_Target
{
    // 블록 기반 좌표로 강약 조절
        float2 blockUV = floor(i.uv * 40.0) / 40.0;

    // 무작위하게 보이는 값 생성 (유사 노이즈)
    float rand = frac(sin(dot(blockUV * _Time.y, float2(12.9898, 78.233))) * 43758.5453);

    // 랜덤이 특정 값보다 크면 강하게 왜곡
    float glitchStrength = (rand > 0.8) ? 1.5 : 1.0;

    // 더 빠르게 진동하는 노이즈 (움직임 속도 상승)
    float fakeNoise = sin(i.uv.y * 500.0 + _Time.y * 120.0);

    float offset = fakeNoise * _GlitchAmount * 0.03 * glitchStrength;

    float r = tex2D(_MainTex, i.uv + float2(offset, 0)).r;
    float g = tex2D(_MainTex, i.uv).g;
    float b = tex2D(_MainTex, i.uv - float2(offset, 0)).b;

    return float4(r, g, b, 1);
}


            ENDCG
        }
    }
}