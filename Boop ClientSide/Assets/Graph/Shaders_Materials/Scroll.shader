﻿Shader "Custom/Scroll"
{
    Properties
    {
        _ColorTint ("Color", Color) = (1, 1, 1, 1)
        _Contrast ("Contrast", Range(0, 1)) = 0.5
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Scroll Speed", vector) = (0, 10, 0, 0)
        _Cutoff ("Cutout", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Pass
        {
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _MainTex_ST;
            float4 _ColorTint;
            float _Contrast;
            float4 _Speed;
            float _Cutoff;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.uv += _Speed * _Time.x;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col = saturate(lerp(half4(0.5, 0.5, 0.5, 0.5), col, _Contrast));
                col *= _ColorTint;
                float newOpacity = tex2D(_MainTex, i.uv).a;

                if (newOpacity < _Cutoff)
                {
                    newOpacity = 0.0;
                }

                return float4 (col.r, col.g, col.b, newOpacity * _ColorTint.a);
            }
            ENDCG
        }
    }
}
