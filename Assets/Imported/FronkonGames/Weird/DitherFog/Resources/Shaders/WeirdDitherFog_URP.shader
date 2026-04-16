////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Shader "Hidden/Fronkon Games/Weird/Dither Fog"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Weird Dither Fog Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      // Dither Fog parameters
      float _FogOpacity;
      int _FogColorMode;  // 0 = Solid, 1 = Auto, 2 = Gradient
      float4 _FogColor;
      float4 _FogGradientNear;
      float4 _FogGradientFar;
      int _DitheringMode;  // 0 = None, 1 = Bayer4x4, 2 = Bayer8x8, 3 = Bayer16x16, 4 = Bayer32x32
      int _DitherScale;    // Scale factor for dither pattern (1-8)
      int _AdaptiveDithering;    // Enable adaptive dithering
      float _AdaptiveBrightnessThreshold;  // Brightness threshold for adaptation
      float _AdaptiveContrastSensitivity;  // Contrast sensitivity for adaptation
      int _Quantize;
      float2 _FogCurve;
      float _FogStart;
      int _CurvedFog;

      // Adaptive dithering analysis functions
      inline float GetLocalBrightness(float2 uv, float radius)
      {
        // Sample multiple points around the current pixel to determine local brightness
        float brightness = 0.0;
        const int samples = 5;

        [unroll]
        for (int i = -2; i <= 2; ++i)
        {
          [unroll]
          for (int j = -2; j <= 2; ++j)
          {
            float2 offset = float2(i, j) * radius / _ScreenParams.xy;
            float3 color = SAMPLE_MAIN(uv + offset).rgb;
            brightness += dot(color, float3(0.299, 0.587, 0.114)); // Luminance
          }
        }

        return brightness / (samples * samples);
      }

      inline float GetLocalContrast(float2 uv, float radius)
      {
        // Calculate contrast as the standard deviation of brightness in a local area
        float meanBrightness = GetLocalBrightness(uv, radius);
        float variance = 0.0;
        const int samples = 5;

        [unroll]
        for (int i = -2; i <= 2; ++i)
        {
          [unroll]
          for (int j = -2; j <= 2; ++j)
          {
            float2 offset = float2(i, j) * radius / _ScreenParams.xy;
            float3 color = SAMPLE_MAIN(uv + offset).rgb;
            float brightness = dot(color, float3(0.299, 0.587, 0.114));
            float diff = brightness - meanBrightness;
            variance += diff * diff;
          }
        }

        return sqrt(variance / (samples * samples));
      }

      inline float GetAdaptiveDitherStrength(float2 uv)
      {
        // Calculate adaptive adjustments based on local content
        float localBrightness = GetLocalBrightness(uv, 2.0);
        float localContrast = GetLocalContrast(uv, 2.0);

        // Brightness adaptation: reduce dithering in dark areas, increase in bright areas
        float brightnessFactor = saturate((localBrightness - _AdaptiveBrightnessThreshold) * 2.0 + 1.0);

        // Contrast adaptation: reduce dithering in high-contrast areas to preserve detail
        float contrastFactor = 1.0 - saturate(localContrast * _AdaptiveContrastSensitivity);

        // Combine factors for final adaptive strength
        return brightnessFactor * contrastFactor;
      }

      // Pre-calculated 4x4 Bayer matrix values
      inline int GetBayer4x4(int x, int y)
      {
        uint lookup[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };
        return lookup[(y % 4) * 4 + (x % 4)];
      }

      inline int GetBayer8x8(uint x, uint y)
      {
        // Use 4x4 Bayer as base and expand
        int x4 = (x % 8) / 2;
        int y4 = (y % 8) / 2;
        int baseValue = GetBayer4x4(x4, y4);
        int quadrantX = (x % 8) % 2;
        int quadrantY = (y % 8) % 2;
        int quadrant = quadrantX + quadrantY * 2;

        // Bayer quadrant ordering: 0, 2, 3, 1
        int quadrantOffset;
        if (quadrant == 0) quadrantOffset = 0;
        else if (quadrant == 1) quadrantOffset = 2;
        else if (quadrant == 2) quadrantOffset = 3;
        else quadrantOffset = 1;

        return baseValue * 4 + quadrantOffset;
      }

      // Get dither threshold based on mode and position
      inline float GetDitherThreshold(int2 pos)
      {
        if (_DitheringMode == 1) // Bayer4x4
        {
          int bayerValue = GetBayer4x4(pos.x, pos.y);
          return (float)(bayerValue + 1) / 16.0;
        }
        else if (_DitheringMode == 2) // Bayer8x8
        {
          int bayerValue = GetBayer8x8(pos.x, pos.y);
          return (float)(bayerValue + 1) / 64.0;
        }
        else if (_DitheringMode == 3) // Bayer16x16 - extend 8x8 pattern
        {
          int x8 = (pos.x % 16) / 2;
          int y8 = (pos.y % 16) / 2;
          int baseValue = GetBayer8x8(x8, y8);
          int quadrantX = (pos.x % 16) % 2;
          int quadrantY = (pos.y % 16) % 2;
          int quadrant = quadrantX + quadrantY * 2;

          int quadrantOffset;
          if (quadrant == 0) quadrantOffset = 0;
          else if (quadrant == 1) quadrantOffset = 2;
          else if (quadrant == 2) quadrantOffset = 3;
          else quadrantOffset = 1;

          int bayerValue = baseValue * 4 + quadrantOffset;
          return (float)(bayerValue + 1) / 256.0;
        }
        else if (_DitheringMode == 4) // Bayer32x32 - extend 16x16 pattern
        {
          int x16 = (pos.x % 32) / 2;
          int y16 = (pos.y % 32) / 2;
          // Get 16x16 value by extending 8x8
          int x8 = x16 / 2;
          int y8 = y16 / 2;
          int baseValue = GetBayer8x8(x8, y8);
          int quadrant8 = ((x16 % 2) + (y16 % 2) * 2);
          int offset8;
          if (quadrant8 == 0) offset8 = 0;
          else if (quadrant8 == 1) offset8 = 2;
          else if (quadrant8 == 2) offset8 = 3;
          else offset8 = 1;
          int value16 = baseValue * 4 + offset8;

          // Now extend to 32x32
          int quadrantX = (pos.x % 32) % 2;
          int quadrantY = (pos.y % 32) % 2;
          int quadrant = quadrantX + quadrantY * 2;
          int quadrantOffset;
          if (quadrant == 0) quadrantOffset = 0;
          else if (quadrant == 1) quadrantOffset = 2;
          else if (quadrant == 2) quadrantOffset = 3;
          else quadrantOffset = 1;

          int bayerValue = value16 * 4 + quadrantOffset;
          return (float)(bayerValue + 1) / 1024.0;
        }

        return 0.5; // Fallback for None mode (shouldn't reach here)
      }

      // Calculate fog amount based on depth
      inline float GetFog(float2 uv)
      {
        float depth = SampleLinear01Depth(uv);

        if (_CurvedFog > 0)
        {
          // Curved fog: distance from a point considering horizontal position
          float2 curvePos = float2((uv.x - 0.5) * 2.0 * depth + 0.5, depth);
          float2 startPos = float2(0.5, _FogStart - 0.45);
          depth = distance(curvePos, startPos);
        }
        else
        {
          // Linear fog: simple distance from fog start
          depth = distance(depth, _FogStart - 0.45);
        }

        return smoothstep(_FogCurve.x, _FogCurve.y, depth);
      }

      // Apply dithering to fog value
      // Adapted from: http://devlog-martinsh.blogspot.com.br/2011/03/glsl-dithering.html
      inline float Dither(float x, float2 uv)
      {
        x *= _FogOpacity;

        // Apply adaptive dithering adjustments if enabled
        if (_AdaptiveDithering > 0)
        {
          float adaptiveStrength = GetAdaptiveDitherStrength(uv);
          x *= adaptiveStrength;
        }
        
        // Apply quantization if enabled
        if (_Quantize > 0)
          x = round(x * _Quantize) / _Quantize;
          
        // Get screen position and apply dither scaling
        int2 pos = int2(uv * _ScreenParams.xy / (float)_DitherScale);
        
        // Get threshold from selected Bayer matrix
        float limit = GetDitherThreshold(pos);

        // Binary dithering: return 0 or 1
        return (x < limit) ? 0.0 : 1.0;
      }

      // Get average scene color by sampling multiple points
      inline float3 GetSceneColor()
      {
        static const int pointCount = 8;
        static const float2 samplePoints[8] = {
          float2(0.0, 0.0),
          float2(0.0, 0.5),
          float2(0.0, 1.0),
          float2(0.5, 0.0),
          float2(0.5, 1.0),
          float2(1.0, 0.0),
          float2(1.0, 0.5),
          float2(1.0, 1.0)
        };

        float3 color = SAMPLE_MAIN_LOD(samplePoints[0]).rgb;

        [unroll]
        for (int i = 1; i < pointCount; ++i)
          color += SAMPLE_MAIN_LOD(samplePoints[i]).rgb;

        return color / (float)pointCount;
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);

        float3 pixel = color.rgb;

        // Calculate fog amount based on depth (0 = near, 1 = far)
        float fog = GetFog(uv);
        float depthFactor = fog; // Store for gradient interpolation before dithering
        
        // Apply dithering or standard fog opacity
        if (_DitheringMode > 0)
          fog = Dither(fog, uv);
        else
          fog *= _FogOpacity;

        // Determine fog color based on color mode
        float3 fogColor;
        float fogAlpha = 1.0;

        if (_FogColorMode == 1) // Auto
          fogColor = GetSceneColor();
        else if (_FogColorMode == 2) // Gradient
        {
          // Interpolate between near and far gradient colors based on depth
          float4 gradientColor = lerp(_FogGradientNear, _FogGradientFar, depthFactor);
          fogColor = gradientColor.rgb;
          fogAlpha = gradientColor.a;
        }
        else // Solid (0)
          fogColor = _FogColor.rgb;

        // Apply fog with alpha consideration for gradient mode
        pixel = lerp(pixel, fogColor, fog * fogAlpha);

        // Color adjustments
        pixel = ColorAdjust(pixel, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

#if 0
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, float4(pixel, 1.0), _Intensity);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
