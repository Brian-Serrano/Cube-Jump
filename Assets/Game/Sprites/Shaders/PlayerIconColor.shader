Shader "Custom/PlayerIconColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PrimaryColor ("Primary Color", Color) = (1, 1, 1, 1)
        _SecondaryColor ("Secondary Color", Color) = (1, 1, 1, 1)

        // Required by Unity UI masking system
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        LOD 100
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            float4 _PrimaryColor;
            float4 _SecondaryColor;
            float4 _MainTex_TexelSize;

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
                fixed4 col = tex2D(_MainTex, i.uv);

                bool hasWhiteNeighbor = false;
                float2 texelSize = _MainTex_TexelSize.xy;

                // Loop over neighboring pixels in a square radius
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        float2 offset = texelSize * float2(x, y);
                        fixed4 neighbor = tex2D(_MainTex, i.uv + offset);

                        if (neighbor.r == 1 && neighbor.g == 1 && neighbor.b == 1 && neighbor.a > 0)
                        {
                            hasWhiteNeighbor = true;
                            break;
                        }
                    }
                    if (hasWhiteNeighbor) break;
                }

                if ((col.r > 0.1 && col.g > 0.1 && col.b > 0.1) && col.a > 0)
                {
                    if ((col.r == 1 && col.g == 1 && col.b == 1) || hasWhiteNeighbor)
                    {
                        return ((col / 0.68) * i.color) * _SecondaryColor;
                    }
                    else
                    {
                        return ((col / 0.68) * i.color) * _PrimaryColor;
                    }
                }
                
                return col * i.color;
            }
            ENDCG
        }
    }
}
