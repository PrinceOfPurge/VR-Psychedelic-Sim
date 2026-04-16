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
Shader "Hidden/Fronkon Games/Weird/Spiral"
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
      Name "Fronkon Games Weird Spiral Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      // Droste effect parameters
      int _ShapeType;       // 0 = Circular, 1 = Rectangular, 2 = Triangular, 3 = Pentagonal, 4 = Hexagonal
      float _SpiralAmount;  // [0, 1] - spiral twist amount
      float _Rotation;      // rotation angle in radians
      float _RotationSpeed; // rotation animation speed
      float _OuterRing;     // [0, 1]
      float _ZoomSpeed;     // [-2, 2] - animation speed
      float _Frequency;     // [0.1, 5]
      float2 _Center;       // [-0.5, 0.5]
      float _Wrap;          // [0, 1] - transition between original and droste
      int _EdgeMode;        // 0 = Clamp, 1 = Mirror, 2 = Repeat

      // Outer tint parameters
      float _OuterTintIntensity;  // [0, 1]
      int _OuterTintColorBlend;
      float3 _OuterTintColor;     // RGB color
      float _OuterTintSoftness;   // [0.1, 5]

      // Shadow parameters
      float _ShadowIntensity;  // [0, 1]
      float3 _ShadowColor;     // RGB color
      float _ShadowSoftness;   // [0.1, 5]
      float _ShadowOffset;     // [-1, 1]
      int _ShadowColorBlend;

      // Line parameters
      float _LineWidth;        // [0, 0.1]
      float3 _LineColor;       // RGB color
      int _LineColorBlend;
      float _LineSoftness;     // [0.01, 1]
      int _LineCount;          // [1, 10]

      // 2D rotation matrix
      inline float2 Rotate2D(float2 p, float angle)
      {
        float s = sin(angle);
        float c = cos(angle);

        return float2(p.x * c - p.y * s, p.x * s + p.y * c);
      }

      // Handle UV edge modes
      inline float2 ApplyEdgeMode(float2 uv, int mode)
      {
        if (mode == 0) // Clamp
          return saturate(uv);
        
        if (mode == 1) // Mirror
        {
          float2 m = frac(uv * 0.5) * 2.0;
          return 1.0 - abs(m - 1.0);
        }
        
        // Repeat
        return frac(uv);
      }

      // Get number of sides for polygon shapes
      inline int GetPolygonSides(int effectType)
      {
        // 0 = Circle (infinite), 1 = Rect (4), 2 = Triangle (3), 3 = Pentagon (5), 4 = Hexagon (6)
        int sides[5] = { 0, 4, 3, 5, 6 };
        
        return sides[effectType];
      }

      // Calculate distance to regular polygon edge from center
      // n = number of sides, angle = current angle in radians
      inline float PolygonDistance(float2 pos, int n)
      {
        float dist = length(pos);
        if (dist < 0.0001) return 0.0;
        
        float angle = atan2(pos.y, pos.x);
        float segmentAngle = PI2 / float(n);
        
        // Find angle within current segment
        float localAngle = fmod(angle + PI2 + segmentAngle * 0.5, segmentAngle) - segmentAngle * 0.5;
        
        // Distance factor for polygon (how much further the edge is compared to inscribed circle)
        float polygonFactor = cos(segmentAngle * 0.5) / cos(localAngle);
        
        return dist / polygonFactor;
      }

      // Calculate unit polygon ratio (distance from center to edge at this angle)
      inline float PolygonUnitRatio(float2 pos, int n)
      {
        float dist = length(pos);
        if (dist < 0.0001) return 1.0;
        
        float angle = atan2(pos.y, pos.x);
        float segmentAngle = PI2 / float(n);
        
        // Find angle within current segment
        float localAngle = fmod(angle + PI2 + segmentAngle * 0.5, segmentAngle) - segmentAngle * 0.5;
        
        // Distance to polygon edge at unit scale
        float edgeDist = 0.5 / cos(localAngle) * cos(segmentAngle * 0.5);
        
        return edgeDist / dist;
      }

      // Fast atan2 approximation
      inline float Atan2Approx(float y, float x)
      {
        float ax = abs(x);
        float ay = abs(y);
        float mx = max(ay, ax);
        float mn = min(ay, ax);
        float a = mn / mx;
        
        a = mx == 0.0 ? 0.0 : a;
        a = isnan(a) ? 0.0 : a;
        a = isinf(a) ? 0.0 : a;
        
        y += 0.0000001;
        
        float sa = saturate(a);
        a = sa == 0.0 ? 0.0 : a;
        a = isnan(a) ? 0.0 : a;
        a = isinf(a) ? 0.0 : a;
        
        float s = a * a;
        float r = ((-0.0464964749 * s + 0.15931422) * s - 0.327622764) * s * a + a;
        r = ay > ax ? 1.57079637 - r : r;
        r = x < 0.0 ? 3.14159274 - r : r;
        r = y < 0.0 ? -r : r;
        
        return r;
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);

        // Calculate aspect ratio based on effect type
        // Circular mode uses screen aspect ratio, polygons use uniform aspect
        float2 aspectRatio = (_ShapeType == 0 || _ShapeType >= 2)
            ? float2(_ScreenParams.x / _ScreenParams.y, 1.0) 
            : float2(1.0, 1.0);

        // Calculate center angle (for off-center projection smoothing)
        float newCenterAngle = abs(_Center.x) + abs(_Center.y) < 0.01 
            ? 1.0 
            : (Atan2Approx(-_Center.x * aspectRatio.x, -_Center.y) + PI) / PI2;

        // Calculate inner ring size based on frequency
        float innerRing = 1.0 / exp(1.0 / _Frequency);

        // Transform coordinate system
        float2 newPos = (uv - 0.5 + _Center) * aspectRatio;

        // Apply rotation (static + animated)
        float totalRotation = _Rotation + _EffectTime.y * _RotationSpeed;
        newPos = Rotate2D(newPos, totalRotation);

        // Calculate orientation of center and pixel
        float newCenterDistance = 1.0 - 2.0 * max(abs(_Center.x), abs(_Center.y));

        // Calculate and normalize angle
        float angle = (Atan2Approx(newPos.x, newPos.y) + PI) / PI2;
        float val = angle * _SpiralAmount;
        angle = 1.0 - fmod(abs(abs(angle - newCenterAngle) - 0.5), 0.5) * 2.0;

        // Smooth off-center projection
        float angleSmooth = (1.0 - cos(angle * angle * PI)) / 2.0;
        float intensityFactor = lerp(newCenterDistance, 1.0, angleSmooth);

        // Calculate distance from center based on shape type
        float vectorLength = sqrt(dot(newPos, newPos));
        float shapeDist;
        float unitShapeRatio;
        
        if (_ShapeType == 0)
        {
          // Circular
          shapeDist = vectorLength / intensityFactor;
          unitShapeRatio = 0.5 / vectorLength;
        }
        else if (_ShapeType == 1)
        {
          // Rectangular (special case - uses max of abs coordinates)
          shapeDist = max(abs(newPos.x), abs(newPos.y));
          unitShapeRatio = 0.5 / max(abs(newPos.x), abs(newPos.y));
        }
        else
        {
          // Polygon shapes (Triangle, Pentagon, Hexagon)
          int sides = GetPolygonSides(_ShapeType);
          shapeDist = PolygonDistance(newPos, sides) / intensityFactor;
          unitShapeRatio = PolygonUnitRatio(newPos, sides);
        }

        // Apply zoom and frequency transformation
        float rcdist = log(shapeDist * 9.0) * _Frequency;
        val += rcdist;
        
        // Apply zoom speed animation - adds continuous time-based offset for tunnel effect
        val += _EffectTime.y * _ZoomSpeed;
        
        // Use frac instead of fmod to ensure positive values and eliminate seam
        val = (exp(frac(val) / _Frequency) - 1.0) / (rcp(innerRing) - 1.0);

        // Calculate normalized vector
        float2 normalized = newPos * unitShapeRatio;

        // Calculate relative position towards outer and inner ring and interpolate
        float innerRingScaled = innerRing * _OuterRing;
        float realScale = lerp(innerRingScaled, _OuterRing, val);
        realScale *= intensityFactor;

        // Calculate final adjusted UV coordinates
        float2 adjustedUV = normalized * realScale / aspectRatio + 0.5 - _Center;

        // Smooth wrap/unwrap transition - interpolate between original UV and droste UV
        float2 finalUV = lerp(uv, adjustedUV, _Wrap);
        
        // Apply edge mode to handle UVs outside [0,1]
        finalUV = ApplyEdgeMode(finalUV, _EdgeMode);
        
        // Also interpolate the recursion depth value for shadow transition
        float finalVal = lerp(0.5, val, _Wrap);

        // Sample the texture at the warped coordinates
        float3 pixel = SAMPLE_MAIN(finalUV).rgb;

        // Apply outer tint based on recursion depth (inverted - affects outer areas)
        if (_OuterTintIntensity > 0.0)
        {
          float tintDepth = pow(1.0 - finalVal, _OuterTintSoftness);
          float tintFactor = tintDepth * _OuterTintIntensity * _Wrap;
          pixel = lerp(pixel, ColorBlend(_OuterTintColorBlend, pixel, _OuterTintColor), tintFactor);
        }

        // Apply shadow effect based on recursion depth
        // val ranges from 0 (outer) to 1 (inner), we use this to create depth shadows
        float shadowDepth = saturate(finalVal + _ShadowOffset);
        float shadowFactor = pow(shadowDepth, _ShadowSoftness) * _Wrap; // Shadow also fades with wrap
        float shadow = 1.0 - (shadowFactor * _ShadowIntensity);
        pixel = lerp(ColorBlend(_ShadowColorBlend, pixel, shadow), lerp(pixel, _ShadowColor, shadowFactor * _ShadowIntensity), _ShadowIntensity * 1.0);

        // Apply lines at recursion boundaries
        // Lines appear where val is close to boundary positions
        if (_LineWidth > 0.0)
        {
          // Scale line width by texel size for resolution independence
          float scaledLineWidth = _LineWidth * TEXEL_SIZE.y * 100.0;
          float scaledSoftness = scaledLineWidth * _LineSoftness;
          
          // Calculate line alpha with multiple lines support
          float lineAlpha = 0.0;
          float lineSpacing = 1.0 / float(_LineCount);
          
          for (int i = 0; i < _LineCount; i++)
          {
            float linePos = float(i) * lineSpacing;
            float distToLine = abs(frac(finalVal + 0.5 - linePos) - 0.5);
            float thisLineAlpha = 1.0 - smoothstep(scaledLineWidth - scaledSoftness, scaledLineWidth, distToLine);
            lineAlpha = max(lineAlpha, thisLineAlpha);
          }
          
          lineAlpha *= _Wrap; // Line also fades with wrap
          pixel = lerp(pixel, ColorBlend(_LineColorBlend, pixel, _LineColor), lineAlpha);
        }

        // Color adjustments
        pixel = ColorAdjust(pixel, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        return lerp(color, float4(pixel, 1.0), _Intensity);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
