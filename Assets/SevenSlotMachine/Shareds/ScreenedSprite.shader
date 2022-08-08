Shader "Unlit/ScreenedSprite"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        // Change 1 of 2:
        // Set blend mode to multiply this sprite's output with the background.
        Blend DstColor OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
    #pragma vertex SpriteVert
    #pragma fragment ScreenSpriteFrag
    #pragma target 2.0
    #pragma multi_compile_instancing
    #pragma multi_compile _ PIXELSNAP_ON
    #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
    #include "UnitySprites.cginc"

            fixed4 ScreenSpriteFrag(v2f IN) : SV_Target
            {
                //fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                // Change 2 of 2:
                // Output inverted colour. (Keeping pre-multiplied alpha)
                //c.rgb = (1.0f - c.rgb) * c.a;
                //Screen(layer, background) = 1 - (1 - layer) * (1 - background)
                //c.rgb = (1.0f - c.rgb) * c.a;
                //return c;

                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }

            ENDCG
        }
    }
}