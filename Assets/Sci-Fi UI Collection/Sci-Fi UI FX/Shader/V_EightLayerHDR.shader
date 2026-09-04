Shader "UIAnimFX/V_EightLayerHDR"
{
    Properties
    {
        _Layer1Tex ("Layer 1 Texture", 2D) = "white" {}
        [HDR] _Layer1Color ("Layer 1 Color", Color) = (1,1,1,1)
        _Layer1Speed ("Layer 1 Move Speed", Float) = 0
        _Layer1Angle ("Layer 1 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer2Tex ("Layer 2 Texture", 2D) = "white" {}
        [HDR] _Layer2Color ("Layer 2 Color", Color) = (1,1,1,1)
        _Layer2Speed ("Layer 2 Move Speed", Float) = 0
        _Layer2Angle ("Layer 2 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer3Tex ("Layer 3 Texture", 2D) = "white" {}
        [HDR] _Layer3Color ("Layer 3 Color", Color) = (1,1,1,1)
        _Layer3Speed ("Layer 3 Move Speed", Float) = 0
        _Layer3Angle ("Layer 3 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer4Tex ("Layer 4 Texture", 2D) = "white" {}
        [HDR] _Layer4Color ("Layer 4 Color", Color) = (1,1,1,1)
        _Layer4Speed ("Layer 4 Move Speed", Float) = 0
        _Layer4Angle ("Layer 4 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer5Tex ("Layer 5 Texture", 2D) = "white" {}
        [HDR] _Layer5Color ("Layer 5 Color", Color) = (1,1,1,1)
        _Layer5Speed ("Layer 5 Move Speed", Float) = 0
        _Layer5Angle ("Layer 5 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer6Tex ("Layer 6 Texture", 2D) = "white" {}
        [HDR] _Layer6Color ("Layer 6 Color", Color) = (1,1,1,1)
        _Layer6Speed ("Layer 6 Move Speed", Float) = 0
        _Layer6Angle ("Layer 6 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer7Tex ("Layer 7 Texture", 2D) = "white" {}
        [HDR] _Layer7Color ("Layer 7 Color", Color) = (1,1,1,1)
        _Layer7Speed ("Layer 7 Move Speed", Float) = 0
        _Layer7Angle ("Layer 7 Move Angle (Degrees)", Range(0, 360)) = 90
        
        _Layer8Tex ("Layer 8 Texture", 2D) = "white" {}
        [HDR] _Layer8Color ("Layer 8 Color", Color) = (1,1,1,1)
        _Layer8Speed ("Layer 8 Move Speed", Float) = 0
        _Layer8Angle ("Layer 8 Move Angle (Degrees)", Range(0, 360)) = 90
        
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
            
            sampler2D _Layer2Tex;
            float4 _Layer2Tex_ST;
            half4 _Layer2Color;
            float _Layer2Speed;
            float _Layer2Angle;
            
            sampler2D _Layer3Tex;
            float4 _Layer3Tex_ST;
            half4 _Layer3Color;
            float _Layer3Speed;
            float _Layer3Angle;
            
            sampler2D _Layer4Tex;
            float4 _Layer4Tex_ST;
            half4 _Layer4Color;
            float _Layer4Speed;
            float _Layer4Angle;
            
            sampler2D _Layer5Tex;
            float4 _Layer5Tex_ST;
            half4 _Layer5Color;
            float _Layer5Speed;
            float _Layer5Angle;
            
            sampler2D _Layer6Tex;
            float4 _Layer6Tex_ST;
            half4 _Layer6Color;
            float _Layer6Speed;
            float _Layer6Angle;
            
            sampler2D _Layer7Tex;
            float4 _Layer7Tex_ST;
            half4 _Layer7Color;
            float _Layer7Speed;
            float _Layer7Angle;
            
            sampler2D _Layer8Tex;
            float4 _Layer8Tex_ST;
            half4 _Layer8Color;
            float _Layer8Speed;
            float _Layer8Angle;
            
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
                float2 layer2UVBase = TRANSFORM_TEX(sampleUV, _Layer2Tex);
                float2 layer3UVBase = TRANSFORM_TEX(sampleUV, _Layer3Tex);
                float2 layer4UVBase = TRANSFORM_TEX(sampleUV, _Layer4Tex);
                float2 layer5UVBase = TRANSFORM_TEX(sampleUV, _Layer5Tex);
                float2 layer6UVBase = TRANSFORM_TEX(sampleUV, _Layer6Tex);
                float2 layer7UVBase = TRANSFORM_TEX(sampleUV, _Layer7Tex);
                float2 layer8UVBase = TRANSFORM_TEX(sampleUV, _Layer8Tex);
                
                bool layer1Valid = layer1UVBase.x != 0.0 && layer1UVBase.x != 1.0;
                bool layer2Valid = layer2UVBase.x != 0.0 && layer2UVBase.x != 1.0;
                bool layer3Valid = layer3UVBase.x != 0.0 && layer3UVBase.x != 1.0;
                bool layer4Valid = layer4UVBase.x != 0.0 && layer4UVBase.x != 1.0;
                bool layer5Valid = layer5UVBase.x != 0.0 && layer5UVBase.x != 1.0;
                bool layer6Valid = layer6UVBase.x != 0.0 && layer6UVBase.x != 1.0;
                bool layer7Valid = layer7UVBase.x != 0.0 && layer7UVBase.x != 1.0;
                bool layer8Valid = layer8UVBase.x != 0.0 && layer8UVBase.x != 1.0;
                
                float2 layer1UV = getAngleBasedUV(layer1UVBase, _Layer1Speed, _Layer1Angle);
                float2 layer2UV = getAngleBasedUV(layer2UVBase, _Layer2Speed, _Layer2Angle);
                float2 layer3UV = getAngleBasedUV(layer3UVBase, _Layer3Speed, _Layer3Angle);
                float2 layer4UV = getAngleBasedUV(layer4UVBase, _Layer4Speed, _Layer4Angle);
                float2 layer5UV = getAngleBasedUV(layer5UVBase, _Layer5Speed, _Layer5Angle);
                float2 layer6UV = getAngleBasedUV(layer6UVBase, _Layer6Speed, _Layer6Angle);
                float2 layer7UV = getAngleBasedUV(layer7UVBase, _Layer7Speed, _Layer7Angle);
                float2 layer8UV = getAngleBasedUV(layer8UVBase, _Layer8Speed, _Layer8Angle);
                
                half4 layer1 = layer1Valid ? tex2D(_Layer1Tex, layer1UV) * _Layer1Color : half4(0,0,0,0);
                half4 layer2 = layer2Valid ? tex2D(_Layer2Tex, layer2UV) * _Layer2Color : half4(0,0,0,0);
                half4 layer3 = layer3Valid ? tex2D(_Layer3Tex, layer3UV) * _Layer3Color : half4(0,0,0,0);
                half4 layer4 = layer4Valid ? tex2D(_Layer4Tex, layer4UV) * _Layer4Color : half4(0,0,0,0);
                half4 layer5 = layer5Valid ? tex2D(_Layer5Tex, layer5UV) * _Layer5Color : half4(0,0,0,0);
                half4 layer6 = layer6Valid ? tex2D(_Layer6Tex, layer6UV) * _Layer6Color : half4(0,0,0,0);
                half4 layer7 = layer7Valid ? tex2D(_Layer7Tex, layer7UV) * _Layer7Color : half4(0,0,0,0);
                half4 layer8 = layer8Valid ? tex2D(_Layer8Tex, layer8UV) * _Layer8Color : half4(0,0,0,0);
                
                float layer1EdgeFade = calculateTextureEdgeFade(layer1UV, _TextureEdgeFade);
                float layer2EdgeFade = calculateTextureEdgeFade(layer2UV, _TextureEdgeFade);
                float layer3EdgeFade = calculateTextureEdgeFade(layer3UV, _TextureEdgeFade);
                float layer4EdgeFade = calculateTextureEdgeFade(layer4UV, _TextureEdgeFade);
                float layer5EdgeFade = calculateTextureEdgeFade(layer5UV, _TextureEdgeFade);
                float layer6EdgeFade = calculateTextureEdgeFade(layer6UV, _TextureEdgeFade);
                float layer7EdgeFade = calculateTextureEdgeFade(layer7UV, _TextureEdgeFade);
                float layer8EdgeFade = calculateTextureEdgeFade(layer8UV, _TextureEdgeFade);
                
                layer1.a *= layer1EdgeFade;
                layer2.a *= layer2EdgeFade;
                layer3.a *= layer3EdgeFade;
                layer4.a *= layer4EdgeFade;
                layer5.a *= layer5EdgeFade;
                layer6.a *= layer6EdgeFade;
                layer7.a *= layer7EdgeFade;
                layer8.a *= layer8EdgeFade;
                
                half4 finalColor = layer1;
                float a;
                float3 rgb;
                
                a = layer2.a + finalColor.a * (1 - layer2.a);
                rgb = (layer2.rgb * layer2.a + finalColor.rgb * finalColor.a * (1 - layer2.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer3.a + finalColor.a * (1 - layer3.a);
                rgb = (layer3.rgb * layer3.a + finalColor.rgb * finalColor.a * (1 - layer3.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer4.a + finalColor.a * (1 - layer4.a);
                rgb = (layer4.rgb * layer4.a + finalColor.rgb * finalColor.a * (1 - layer4.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer5.a + finalColor.a * (1 - layer5.a);
                rgb = (layer5.rgb * layer5.a + finalColor.rgb * finalColor.a * (1 - layer5.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer6.a + finalColor.a * (1 - layer6.a);
                rgb = (layer6.rgb * layer6.a + finalColor.rgb * finalColor.a * (1 - layer6.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer7.a + finalColor.a * (1 - layer7.a);
                rgb = (layer7.rgb * layer7.a + finalColor.rgb * finalColor.a * (1 - layer7.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
                a = layer8.a + finalColor.a * (1 - layer8.a);
                rgb = (layer8.rgb * layer8.a + finalColor.rgb * finalColor.a * (1 - layer8.a)) / max(a, 0.0001);
                finalColor = half4(rgb, a);
                
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