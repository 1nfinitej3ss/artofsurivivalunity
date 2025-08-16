Shader "Hidden/VideoFeather"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FeatherAmount ("Feather Amount", Range(0, 1)) = 0.1
        _Softness ("Softness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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
            float _FeatherAmount;
            float _Softness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate edge fading
                float2 border = float2(
                    min(i.uv.x, 1 - i.uv.x),  // distance from left/right edges
                    min(i.uv.y, 1 - i.uv.y)   // distance from top/bottom edges
                );
                
                float fadeX = smoothstep(0, _FeatherAmount, border.x);
                float fadeY = smoothstep(0, _FeatherAmount, border.y);
                
                float alpha = fadeX * fadeY;
                alpha = pow(alpha, _Softness * 2); // Adjust fade intensity
                
                return fixed4(col.rgb, col.a * alpha);
            }
            ENDCG
        }
    }
} 