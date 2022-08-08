Shader "Unlit/SpriteMat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [NoScaleOffset] _OverlayTex ("Overlays Texture", 2D) = "White"{} // Don't display the tiling/offset here, we'll add them in the next lines

        _OverlayST_0 ("Layer 1 Tiling Offset", Vector) = (1,1,0,0) // Express each tiling/offset as vector4 (tiling X & Y ; offset Z & W)
        _OverlayColor_0 ("Layer 1 Color", Color) = (1,0,0,1) // Overlay Layer color

        _OverlayST_1 ("Layer 2 Tiling Offset", Vector) = (1,1,0,0)
        _OverlayColor_1 ("Layer 2 Color", Color) = (0,1,0,1)

        _OverlayST_2 ("Layer 3 Tiling Offset", Vector) = (1,1,0,0)
        _OverlayColor_2 ("Layer 3 Color", Color) = (0,0,1,1)

        _OverlayST_3 ("Layer 3 Tiling Offset", Vector) = (1,1,0,0)
        _OverlayColor_3 ("Layer 3 Color", Color) = (0,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata // vertex shader input
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f // vertex to fragment data
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                float4 uvOverlay01 : TEXCOORD1; // uvs of layer 0 (xy) and 1 (zw) with tiling and offset applied
                float4 uvOverlay23 : TEXCOORD2; // uvs of layer 2 (xy) and 3 (zw) with tiling and offset applied
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OverlayTex;
            float4 _OverlayST_0;
            float4 _OverlayColor_0;
            float4 _OverlayST_1;
            float4 _OverlayColor_1;
            float4 _OverlayST_2;
            float4 _OverlayColor_2;
            float4 _OverlayST_3;
            float4 _OverlayColor_3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.uvOverlay01 = float4(
                    v.uv * _OverlayST_0.xy + _OverlayST_0.zw,
                    v.uv * _OverlayST_1.xy + _OverlayST_1.zw
                );
                o.uvOverlay23 = float4(
                    v.uv * _OverlayST_2.xy + _OverlayST_2.zw,
                    v.uv * _OverlayST_3.xy + _OverlayST_3.zw
                );

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the main texture, never modify the alpha later on
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply each layers:
                // ---- 0 ----
                fixed l0 = tex2D(_OverlayTex, i.uvOverlay01.xy).r; // Get the layer opacity from texture channel
                col.rgb = lerp( col.xyz, _OverlayColor_0.rgb, l0 * _OverlayColor_0.a ); // lerp with the previous color and take into account the layer color opacity

                // ---- 1 ----
                fixed l1 = tex2D(_OverlayTex, i.uvOverlay01.zw).g;
                col.rgb = lerp( col.xyz, _OverlayColor_1.rgb, l1 * _OverlayColor_1.a );

                // ---- 2 ----
                fixed l2 = tex2D(_OverlayTex, i.uvOverlay23.xy).b;
                col.rgb = lerp( col.xyz, _OverlayColor_2.rgb, l2 * _OverlayColor_2.a );

                // ---- 3 ----
                fixed l3 = tex2D(_OverlayTex, i.uvOverlay23.zw).a;
                col.rgb = lerp( col.xyz, _OverlayColor_3.rgb, l3 * _OverlayColor_3.a );

                return col;
            }
            ENDCG
        }
    }
}
