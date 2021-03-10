Shader "Unlit/UnitHighlight"
{
    Properties
    {
        _Color ("Color (RGBA)", Color) = (1, 1, 1, 1)
        _Thickness ("Thickness", Range (0,1)) = 0.45
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            fixed _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half distFromMiddle = distance(i.uv, float2(0.5, 0.5));
                float circleFactor = 1 - step(0.5, distFromMiddle); // outer circle
                fixed thickness = (1 - _Thickness) * 0.5;
                circleFactor = circleFactor - 1 + step(thickness, distFromMiddle); // outer circle - inner circle

                fixed4 col = _Color * circleFactor;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
