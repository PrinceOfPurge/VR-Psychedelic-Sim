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
Shader "Hidden/Fronkon Games/Weird/Kaleidoscope"
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
      Name "Fronkon Games Weird Kaleidoscope Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      float2 _Center;
      int _IterationCount;
      float _Speed;
      float _Scale;
      float _KeepAspectRatio;
      sampler2D _StrengthCurve;

      float _OffsetIntensity;
      float _OffsetScale;
      float2 _OffsetRedScale;
      float2 _OffsetGreenScale;
      float2 _OffsetBlueScale;
      
      float _ColorIntensity;
      int _ColorPalette;
      int _Blend;

      float _SegmentIntensity;
      float3 _SegmentColor;
      int _SegmentBlend;
      float _SegmentWidth;

      float hash12(float2 p)
      {
        float3 q = frac(float3(p.xyx) * float3(5717.0 / 2048.0 * cos(_EffectTime.y * _Speed) * 0.00001, 6451.0 / 4096.0, 1249.0 / 512.0) + float3(0.5, 0.125, 0.25));
        q += dot(q.xz, p.yx) * 0.0156253;

        return frac(dot(q + p.y * 0.38325 * cos(_EffectTime.y * _Speed) * 0.0001, float3(p.xy, p.x) / 127.0) + dot(q, float3(2.0 * sin(_EffectTime.y * _Speed) * 0.001, 0.25, 0.125 - p.x)) + 0.4);
      }

      float2x2 rot(float a)
      {
        float s = sin(a);
        float c = cos(a);
        return float2x2(c, -s, s, c);
      }

      float3 Color(float v)
      {
        v = frac(v * 13.1257) * 10.0;
        
        // Original palette (default behavior)
        if (_ColorPalette == 0)
        {
          float3 c = normalize(float3(1.0, 1.0, 1.0));
          c.xy = mul(c.xy, rot(v));
          c.yz = mul(c.yz, rot(v * 2.0));
          return lerp(float3(c.g, c.g, c.g), c, 1.0 + frac(v * 5.0) * 2.0);
        }
        // Rainbow palette
        else if (_ColorPalette == 1)
        {
          float hue = frac(v * 0.1);
          return HsvToRgb(float3(hue, 1.0, 1.0));
        }
        // Black and White palette
        else if (_ColorPalette == 2)
        {
          float gray = frac(v * 0.5);
          return float3(gray, gray, gray);
        }
        // Neon palette
        else if (_ColorPalette == 3)
        {
          float hue = frac(v * 0.15);
          return HsvToRgb(float3(hue, 1.0, 1.0)) * 1.5;
        }
        // Pastel palette
        else if (_ColorPalette == 4)
        {
          float hue = frac(v * 0.12);
          return HsvToRgb(float3(hue, 0.5, 0.9));
        }
        // Fire palette
        else if (_ColorPalette == 5)
        {
          float t = frac(v * 0.2);
          return lerp(float3(1.0, 0.0, 0.0), lerp(float3(1.0, 0.5, 0.0), float3(1.0, 1.0, 0.0), t), t);
        }
        // Ocean palette
        else if (_ColorPalette == 6)
        {
          float t = frac(v * 0.15);
          return lerp(float3(0.0, 0.3, 0.6), lerp(float3(0.0, 0.6, 0.8), float3(0.2, 0.8, 1.0), t), t);
        }
        // Sunset palette
        else if (_ColorPalette == 7)
        {
          float t = frac(v * 0.18);
          return lerp(float3(1.0, 0.3, 0.0), lerp(float3(1.0, 0.6, 0.2), float3(1.0, 0.9, 0.5), t), t);
        }
        // Monochrome palette (sepia/warm tone)
        else if (_ColorPalette == 8)
        {
          float t = frac(v * 0.3);
          // Create sepia/warm monochrome effect - warm brownish tone
          float gray = t;
          return saturate(float3(gray * 1.15, gray * 0.9, gray * 0.75)); // Warm sepia/brown tone
        }
        // Cyberpunk palette
        else if (_ColorPalette == 9)
        {
          float hue = frac(v * 0.1 + 0.6); // Shift towards purple/cyan
          float3 rgb = HsvToRgb(float3(hue, 0.8, 1.0));
          return rgb * float3(1.2, 0.8, 1.5); // Boost blue/purple
        }
        // Forest palette
        else if (_ColorPalette == 10)
        {
          float t = frac(v * 0.2);
          return lerp(float3(0.0, 0.5, 0.0), lerp(float3(0.0, 0.8, 0.0), float3(0.2, 1.0, 0.2), t), t);
        }
        
        // Fallback to original
        float3 c = normalize(float3(1.0, 1.0, 1.0));
        c.xy = mul(c.xy, rot(v));
        c.yz = mul(c.yz, rot(v * 2.0));
        return lerp(float3(c.g, c.g, c.g), c, 1.0 + frac(v * 5.0) * 2.0);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 originalUV = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 original = SAMPLE_MAIN(originalUV);
        float3 pixel = original.rgb;
        float2 uv = originalUV;

        float2 aspectScale = _KeepAspectRatio > 0.5 ? (_ScreenParams.xy / _ScreenParams.y) : float2(1.0, 1.0);

        float2 toCenter = uv - _Center;
        // Apply aspect ratio correction to distance calculation
        float2 toCenterScaled = toCenter * aspectScale;
        float2 cornerDistances = float2(
          max(_Center.x, 1.0 - _Center.x),
          max(_Center.y, 1.0 - _Center.y));
        float2 cornerDistancesScaled = cornerDistances * aspectScale;
        float maxDist = length(cornerDistancesScaled);
        float normalizedDist = saturate(length(toCenterScaled) / maxDist);
        normalizedDist = 1.0 - normalizedDist;
        normalizedDist *= _Scale;

        float strength = tex2D(_StrengthCurve, float2(normalizedDist, 0.0)).r;

        uv = (uv - _Center) * aspectScale;
        float2 uv2 = uv;

        float rep = 1.5;
        float3 r2 = normalize(float3(uv.xy, 1.0 - dot(uv.xy, uv.xy) * 10.5));
        r2.x += cos(_EffectTime.y * _Speed) * 0.1;
        r2.y += sin(_EffectTime.y * _Speed) * 0.1;
        
        float r3 = normalize(float3(length(uv), 0.1, 0.51)).x;
        float a3 = log2(r3) - _EffectTime.y * _Speed * 0.15222;
        a3 *= rep / PI;
        float theta = atan2(uv.y, uv.x);
        theta *= rep / PI;
        float2 polarUV = float2(theta, a3);

        uv = frac(polarUV);

        float res = 0.0;
        float o = 1.0;
        float s = 1.0;
        float2x2 r = rot(2.0);

        for (int i = 0; i < _IterationCount; ++i)
        {
          o *= s;
          uv = mul(uv, r);

          float2 _uv = mul(uv, rot(res * 8.0)) + r2.xy;
          float fw = fwidth(length(_uv));
          s = min(s, 1.75 + min(frac(_uv.x), frac(_uv.y)) / fw * 0.05);
          float2 fuv = 1.0 - abs(frac(_uv) * 0.5 - 1.0);
          o = min(o, min(fuv.x, fuv.y) / fw * _SegmentWidth);
          res = hash12(floor(_uv));
          uv = uv * 2.0 + 5.0;
        }

        float3 kaleidoscope = clamp(lerp(Color(res) * res * 2.0, float3(res, res, res), 0.1), 0.0, 1.0) * o;
        kaleidoscope *= strength;

        // Offset UV
        float2 offsetRed = lerp(float2(0.0, 0.0), kaleidoscope.r * _OffsetRedScale * _OffsetScale * TEXEL_SIZE.xy, _OffsetIntensity);
        float2 offsetGreen = lerp(float2(0.0, 0.0), kaleidoscope.g * _OffsetGreenScale * _OffsetScale * TEXEL_SIZE.xy, _OffsetIntensity);
        float2 offsetBlue = lerp(float2(0.0, 0.0), kaleidoscope.b * _OffsetBlueScale * _OffsetScale * TEXEL_SIZE.xy, _OffsetIntensity);

        float3 redColor = SAMPLE_MAIN(originalUV + offsetRed).rgb;
        float3 greenColor = SAMPLE_MAIN(originalUV + offsetGreen).rgb;
        float3 blueColor = SAMPLE_MAIN(originalUV + offsetBlue).rgb;

        float3 offset = lerp(redColor, lerp(greenColor, blueColor, 0.5), 0.5);

        // Color
        float3 color = ColorBlend(_Blend, offset, kaleidoscope);
        color = ColorAdjust(color, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        pixel = lerp(offset, color, _ColorIntensity * strength);

        // Segment color
        float segment = (1.0 - o) * strength;
        float3 segments = segment * _SegmentColor;
        segments = ColorBlend(_SegmentBlend, original.rgb, segments);

        pixel = lerp(pixel, segments, segment * _SegmentIntensity);

        return lerp(original, float4(pixel, 1.0), _Intensity);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
