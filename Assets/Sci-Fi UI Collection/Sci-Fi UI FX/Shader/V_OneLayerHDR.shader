Shader "UIAnimFX/V_OneLayerHDR"
{
    Properties
    {
        _Layer1Tex ("Layer Texture", 2D) = "white" {}
        [HDR] _Layer1Color ("Layer Color", Color) = (1,1,1,1)
        _Layer1Speed ("Layer Move Speed", Float) = 0
        _Layer1Angle ("Layer Move Angle (Degrees)", Range(0, 360)) = 90
        
        _ImageEdgeFade ("Image Edge Fade", Range(0.0, 1)) = 0
        _TextureEdgeFade ("Texture Edge Fade", Range(0.0, 1)) = 0
        
        _UVUpMin ("UV Up Min (at y=1)", Range(0.0, 1.0)) = 0.0
        _UVUpMax ("UV Up Max (at y=1)", Range(0.0, 1.0)) = 1.0
        _UVBottomMin ("UV Bottom Min (at y=0)", Range(0.0, 1.0)) = 0.0
        _UVBottomMax ("UV Bottom Max (at y=0)", Range(0.0, 1.0)) = 1.0
        
        [Toggle] _ApplyFinalUVCompression ("Apply Final UV Compression", Float) = 1
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 screenUV : TEXCOORD2;      
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            sampler2D _Layer1Tex;
            float4 _Layer1Tex_ST;
            half4 _Layer1Color;
            float _Layer1Speed;
            float _Layer1Angle;
            
            float _ImageEdgeFade;
            float _TextureEdgeFade;
            float _UVUpMin;
            float _UVUpMax;
            float _UVBottomMin;
            float _UVBottomMax;
            float _ApplyFinalUVCompression;
            
            #define PI 3.14159265359
            
            float2 remapUV(float2 uv) {
                float2 remappedUV = uv;
                remappedUV.y = uv.y;
                float minX = lerp(_UVBottomMin, _UVUpMin, uv.y);
                float maxX = lerp(_UVBottomMax, _UVUpMax, uv.y);
                remappedUV.x = lerp(minX, maxX, uv.x);
                return remappedUV;
            }
            
            float2 inverseRemapUV(float2 compressedUV) {
                float2 originalUV = compressedUV;
                originalUV.y = compressedUV.y;
                float minX = lerp(_UVBottomMin, _UVUpMin, compressedUV.y);
                float maxX = lerp(_UVBottomMax, _UVUpMax, compressedUV.y);
                if (abs(maxX - minX) > 0.0001) {
                    originalUV.x = (compressedUV.x - minX) / (maxX - minX);
                }
                return originalUV;
            }
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.screenUV = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }
            
            float2 getAngleBasedUV(float2 uv, float speed, float angleDegrees) {
                
                float angleRad = angleDegrees * PI / 180.0;
                
                float2 dir = float2(cos(angleRad), sin(angleRad));
                
                float2 offset = dir * speed * _Time.y;
                
                return float2(frac(uv.x + offset.x), frac(uv.y + offset.y));
            }
            
            float calculateTextureEdgeFade(float2 uv, float edgeFadeFactor) {
                
                float2 center = float2(0.5, 0.5);
                float2 distFromCenter = abs(uv - center) * 2.0;
                
                float totalDist = length(distFromCenter);
                
                float edgeFade = totalDist - edgeFadeFactor;
                
                return 1.0 - saturate(edgeFade);
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 sampleUV;
                
                if (_ApplyFinalUVCompression > 0.5) {
                    sampleUV = inverseRemapUV(IN.screenUV);
                    if (sampleUV.x < 0.0 || sampleUV.x > 1.0) {
                        return half4(0, 0, 0, 0);
                    }
                    sampleUV = saturate(sampleUV);
                }
                else {
                    sampleUV = IN.texcoord;
                }
                
                float2 layer1UVBase = TRANSFORM_TEX(sampleUV, _Layer1Tex);
                
                bool layer1Valid = layer1UVBase.x != 0.0 && layer1UVBase.x != 1.0;
                
                float2 layer1UV = getAngleBasedUV(layer1UVBase, _Layer1Speed, _Layer1Angle);
                
                half4 layer1 = layer1Valid ? tex2D(_Layer1Tex, layer1UV) * _Layer1Color : half4(0,0,0,0);
                
                float layer1EdgeFade = calculateTextureEdgeFade(layer1UV, _TextureEdgeFade);
                
                layer1.a *= layer1EdgeFade;
                
                half4 finalColor = layer1;
                
                finalColor *= IN.color;

                float imageEdgeFade = calculateTextureEdgeFade(sampleUV, _ImageEdgeFade);
                
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                finalColor.a *= imageEdgeFade;
                
                return finalColor;
            }
            ENDCG
        }
    }
}