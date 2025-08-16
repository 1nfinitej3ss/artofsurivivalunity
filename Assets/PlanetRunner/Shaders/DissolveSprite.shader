Shader "Custom/DissolveSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount("Dissolve Amount", Range(0, 1)) = 0
        _Fade("Fade Amount", Range(0, 1)) = 1
        _EdgeWidth("Edge Width", Range(0, 1)) = 0.1
        _Color("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _DissolveTexture;
            float _DissolveAmount;
            float _Fade;
            float _EdgeWidth;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float dissolve = tex2D(_DissolveTexture, i.uv).r;
                
                float edge = dissolve - _DissolveAmount;
                float alpha = smoothstep(0, _EdgeWidth, edge);
                
                // Maintain full opacity but fade between sprites
                col.a *= alpha * _Fade;
                
                // Ensure we don't get transparent areas
                col.a = saturate(col.a);
                
                return col;
            }
            ENDCG
        }
    }
} 