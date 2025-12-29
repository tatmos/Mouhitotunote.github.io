Shader "Custom/BackgroundDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1.0
        _DistortionFrequency ("Distortion Frequency", Range(0, 20)) = 10.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DistortionStrength;
            float _DistortionSpeed;
            float _DistortionFrequency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // ゆらゆらさせるためのオフセット計算
                float time = _Time.y * _DistortionSpeed;
                float offsetX = sin(uv.y * _DistortionFrequency + time) * _DistortionStrength;
                float offsetY = cos(uv.x * _DistortionFrequency + time) * _DistortionStrength;
                
                uv.x += offsetX;
                uv.y += offsetY;
                
                fixed4 col = tex2D(_MainTex, uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}
