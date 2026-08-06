Shader "UIRotateScaleFX/TwoLayerHDR"
{
    Properties
    {
        _Layer1Tex ("Layer 1 Texture", 2D) = "white" {}
        [HDR] _Layer1Color ("Layer 1 Color", Color) = (1,1,1,1)
        _Layer1ScaleLifeTime ("Layer 1 Scale Cycle Time", Float) = 1
        [Enum(ScaleOut,0,ScaleIn,1)] _Layer1ScaleDirection ("Layer 1 Scale Direction", Int) = 0
        _Layer1RotAngleSpeed ("Layer 1 Rotation Speed (Degrees/sec)", Float) = 0
        _Layer1DeltaTime ("Layer 1 Start Time Offset", Float) = 0

        _Layer2Tex ("Layer 2 Texture", 2D) = "white" {}
        [HDR] _Layer2Color ("Layer 2 Color", Color) = (1,1,1,1)
        _Layer2ScaleLifeTime ("Layer 2 Scale Cycle Time", Float) = 1
        [Enum(ScaleOut,0,ScaleIn,1)] _Layer2ScaleDirection ("Layer 2 Scale Direction", Int) = 0
        _Layer2RotAngleSpeed ("Layer 2 Rotation Speed (Degrees/sec)", Float) = 0
        _Layer2DeltaTime ("Layer 2 Start Time Offset", Float) = 0

        _ImageEdgeFade ("Image Edge Fade", Range(0.0, 1)) = 0
        _TextureEdgeFade ("Texture Edge Fade", Range(0.0, 1)) = 0

        _MinScale ("Min Scale", Float) = 0
        _MinScaleFade ("Min Scale Fade", Range(0.0, 1.0)) = 0.0
        _MaxScale ("Max Scale", Float) = 1
        _MaxScaleFade ("Max Scale Fade", Range(0.0, 1.0)) = 1.0
        [Enum(Loop,1,Once,0)] _Loop ("Animation Mode", Int) = 1
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ScaleInfo
            {
                float scale;
                float fadeAlpha;
            };

            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            sampler2D _Layer1Tex;
            half4 _Layer1Color;
            float _Layer1ScaleLifeTime;
            int _Layer1ScaleDirection;
            float _Layer1RotAngleSpeed;
            float _Layer1DeltaTime;

            sampler2D _Layer2Tex;
            half4 _Layer2Color;
            float _Layer2ScaleLifeTime;
            int _Layer2ScaleDirection;
            float _Layer2RotAngleSpeed;
            float _Layer2DeltaTime;

            float _ImageEdgeFade;
            float _TextureEdgeFade;
            float _MaxScaleFade;
            float _MinScaleFade;

            float _MinScale;
            float _MaxScale;
            int _Loop;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            ScaleInfo getSawtoothScale(float lifeTime, int direction, float minS, float maxS, float deltaTime)
            {
                ScaleInfo result;
                
                // Check if minScale and maxScale are equal
                if (abs(minS - maxS) < 0.0001)
                {
                    // If equal, don't perform scale animation, just use fixed scale
                    result.scale = minS; // or maxS, they're equal
                    result.fadeAlpha = 1.0; // No fadeout
                    return result;
                }
                
                // Prevent division by zero
                float cycleTime = max(abs(lifeTime), 0.0001);
                float speed = 1.0 / cycleTime;
                
                if (lifeTime <= 0) {
                    result.scale = direction == 0 ? minS : maxS; // Fixed initial value when lifetime is 0
                    result.fadeAlpha = 1.0; // No fadeout
                    return result;
                }
                
                float t = (_Time.y + deltaTime) * speed;
                float p;
                
                if (_Loop == 1)
                {
                    p = frac(t); // 0-1 loop
                }
                else
                {
                    p = saturate(t); // 0-1 one-time interpolation
                }
                
                // Determine scale value
                result.scale = direction == 0 ? lerp(minS, maxS, p) : lerp(maxS, minS, p);
                
                // Calculate fadeout effect - transparent when close to maxScale or minScale
                float scaleRange = maxS - minS;
                float normalizedScale = (result.scale - minS) / max(scaleRange, 0.0001);
                
                float maxFadeAlpha = 1.0;
                float minFadeAlpha = 1.0;
                
                // Transparency when close to maximum value
                if (_MaxScaleFade > 0) {
                    float maxDistance = normalizedScale;  // Normalized distance to max value
                    if (maxDistance > (1.0 - _MaxScaleFade)) {
                        // Calculate transparency when close to max value
                        float maxFadeProgress = (maxDistance - (1.0 - _MaxScaleFade)) / _MaxScaleFade;
                        maxFadeAlpha = 1.0 - maxFadeProgress;
                    }
                }
                
                // Transparency when close to minimum value
                if (_MinScaleFade > 0) {
                    float minDistance = normalizedScale;  // Normalized distance to min value
                    if (minDistance < _MinScaleFade) {
                        // Calculate transparency when close to min value
                        float minFadeProgress = minDistance / _MinScaleFade;
                        minFadeAlpha = minFadeProgress;
                    }
                }
                
                // Use the smaller of the two alpha values as the final transparency
                result.fadeAlpha = min(maxFadeAlpha, minFadeAlpha);
                
                return result;
            }

            float2 rotateAndScaleUV(float2 uv, float scaleLifeTime, int scaleDirection, float rotSpeed, float deltaTime, out float fadeAlpha)
            {
                float time = _Time.y + deltaTime;

                // Move center
                uv -= 0.5;

                ScaleInfo scaleInfo = getSawtoothScale(scaleLifeTime, scaleDirection, _MinScale, _MaxScale, deltaTime);
                fadeAlpha = scaleInfo.fadeAlpha;
                
                // Only scale when scale is not 0, avoid division by zero
                if (abs(scaleInfo.scale) > 0.0001) {
                    uv /= scaleInfo.scale;
                }

                float angle = radians(time * rotSpeed);
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2x2 rot = float2x2(cosA, -sinA, sinA, cosA);
                uv = mul(rot, uv);

                uv += 0.5;
                return uv;
            }

            float calculateTextureEdgeFade(float2 uv, float edgeFadeFactor)
            {
                float2 center = float2(0.5, 0.5);
                float2 distFromCenter = abs(uv - center) * 2.0;
                float totalDist = length(distFromCenter);
                float edgeFade = totalDist - edgeFadeFactor;
                return 1.0 - saturate(edgeFade);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float fadeAlpha1, fadeAlpha2;

                float2 uv1 = rotateAndScaleUV(uv, _Layer1ScaleLifeTime, _Layer1ScaleDirection, _Layer1RotAngleSpeed, _Layer1DeltaTime, fadeAlpha1);
                float2 uv2 = rotateAndScaleUV(uv, _Layer2ScaleLifeTime, _Layer2ScaleDirection, _Layer2RotAngleSpeed, _Layer2DeltaTime, fadeAlpha2);
                
                // Check if UVs are valid (not on the exact edge of the texture)
                bool layer1Valid = uv1.x > 0.0 && uv1.x < 1.0 && uv1.y > 0.0 && uv1.y < 1.0;
                bool layer2Valid = uv2.x > 0.0 && uv2.x < 1.0 && uv2.y > 0.0 && uv2.y < 1.0;

                // Initialize with transparent color
                half4 c1 = half4(0, 0, 0, 0);
                half4 c2 = half4(0, 0, 0, 0);
                
                // Only sample and apply color for valid layers
                if (layer1Valid) {
                    c1 = tex2D(_Layer1Tex, uv1) * _Layer1Color;
                    c1.a *= calculateTextureEdgeFade(uv1, _TextureEdgeFade) * fadeAlpha1;
                }
                
                if (layer2Valid) {
                    c2 = tex2D(_Layer2Tex, uv2) * _Layer2Color;
                    c2.a *= calculateTextureEdgeFade(uv2, _TextureEdgeFade) * fadeAlpha2;
                }

                // Blend layers
                half4 result = c1;
                float a;
                float3 rgb;

                a = c2.a + result.a * (1 - c2.a);
                rgb = (c2.rgb * c2.a + result.rgb * result.a * (1 - c2.a)) / max(a, 0.0001);
                result = half4(rgb, a);

                result *= IN.color;

                float edgeFade = calculateTextureEdgeFade(uv, _ImageEdgeFade);
                result.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                result.a *= edgeFade;

                return result;
            }
            ENDCG
        }
    }
}