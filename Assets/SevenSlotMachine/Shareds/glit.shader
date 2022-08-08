// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Sprites/ShinyDefault2"
 {
     Properties
     {
         ///*
         [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
         _ShineLocation("ShineLocation", Range(0,1)) = 0
         _ShineWidth("ShineWidth", Range(0,1)) = 0
         [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

         _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 1
         _StencilOp ("Stencil Operation", Float) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _ColorMask ("Color Mask", Float) = 15
         //*/

         /*
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _Color("Tint", Color) = (1,1,1,1)
         _ShineLocation("ShineLocation", Range(0,1)) = 0
         _ShineWidth("ShineWidth", Range(0,1)) = 0
         [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

         _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 0
         _StencilOp ("Stencil Operation", Float) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _ColorMask ("Color Mask", Float) = 15
         */
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

     // required for UI.Mask

         ///*
         Stencil
         {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp]
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
         }
         ColorMask [_ColorMask]
         //*/

//         Cull Off
         Lighting Off
         ZWrite Off
         Blend One OneMinusSrcAlpha

         Pass
     {
         CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ PIXELSNAP_ON
 #include "UnityCG.cginc"

     struct appdata_t
     {
         float4 vertex   : POSITION;
         float4 color    : COLOR;
         float2 texcoord : TEXCOORD0;
     };

     struct v2f
     {
         float4 vertex   : SV_POSITION;
         fixed4 color : COLOR;
         float2 texcoord  : TEXCOORD0;
     };

     fixed4 _Color;

     v2f vert(appdata_t IN)
     {
         v2f OUT;
         OUT.vertex = UnityObjectToClipPos(IN.vertex);
         OUT.texcoord = IN.texcoord;
         OUT.color = IN.color * _Color;
 #ifdef PIXELSNAP_ON
         OUT.vertex = UnityPixelSnap(OUT.vertex);
 #endif

         return OUT;
     }

     sampler2D _MainTex;
     sampler2D _AlphaTex;
     float _AlphaSplitEnabled;
     float _ShineLocation;
     float _ShineWidth;

     fixed4 SampleSpriteTexture(float2 uv)
     {
         fixed4 color = tex2D(_MainTex, uv);

 #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
         if (_AlphaSplitEnabled)
             color.a = tex2D(_AlphaTex, uv).r;
 #endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED




         float lowLevel = _ShineLocation - _ShineWidth;
         float highLevel = _ShineLocation + _ShineWidth;
         float currentDistanceProjection = (uv.x + uv.y) / 2;
         if (currentDistanceProjection > lowLevel && currentDistanceProjection < highLevel) {
             float whitePower = 1- (abs(currentDistanceProjection - _ShineLocation ) / _ShineWidth);
             color.rgb +=  color.a * whitePower;
         }

         return color;
     }

     fixed4 frag(v2f IN) : SV_Target
     {
         fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
         c.rgb *= c.a;

     return c;
     }
         ENDCG
     }
     }
 }