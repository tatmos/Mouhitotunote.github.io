Shader "UI/LEDGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0, 1, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _GlowPower ("Glow Power", Range(0.1, 10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
        LOD 100
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float2 center : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _MainColor;
            float _GlowIntensity;
            float _GlowPower;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                // 中心を計算（UV座標の中心は0.5, 0.5）
                o.center = float2(0.5, 0.5);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャをサンプリング（UI Imageのデフォルトテクスチャは白）
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // 中心からの距離を計算
                float2 dist = i.uv - i.center;
                float distance = length(dist);
                
                // 距離に基づいてグラデーション効果（中心が明るく、端が暗い）
                float gradient = 1.0 - saturate(distance * 2.0);
                gradient = pow(gradient, _GlowPower);
                
                // パルス効果
                float pulse = 1.0;
                if (_PulseSpeed > 0)
                {
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                }
                
                // 最終的な色と強度
                fixed4 col = _MainColor * i.color * tex;
                col.rgb *= _GlowIntensity * gradient * pulse;
                col.a *= gradient * tex.a * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
