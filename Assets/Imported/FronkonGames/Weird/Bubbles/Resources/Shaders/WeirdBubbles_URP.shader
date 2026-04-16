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
Shader "Hidden/Fronkon Games/Weird/Bubbles URP"
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
      Name "Fronkon Games Weird Bubbles Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile ___ USE_BLUR

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      // Bubbles parameters
      float _Cell;
      float _BevelK;
      float _BlurPx;
      float _EV;
      float _Spacing;
      float _BubbleRoundness;
      float _SpecPwr;
      float _SpecInt;
      int _BackgroundBlend;
      float3 _BackgroundColor;
      float3 _BubbleColor;
      int _BubbleColorBlend;
      float3 _LightColor;
      float _LightAngle;
      float _LightElevation;

      // Gaussian blur weights
      static const float w0 = 0.2270270270;
      static const float w1 = 0.1945945946;
      static const float w2 = 0.1216216216;
      static const float w3 = 0.0540540541;
      static const float w4 = 0.0162162162;

      // Horizontal blur
      float3 BlurX(float2 uv, float2 res, float radiusPx)
      {
        float2 off = float2(radiusPx / res.x, 0.0);
        float3 c = SAMPLE_MAIN(uv).rgb * w0;
        c += SAMPLE_MAIN(uv + off * 1.0).rgb * w1;
        c += SAMPLE_MAIN(uv - off * 1.0).rgb * w1;
        c += SAMPLE_MAIN(uv + off * 2.0).rgb * w2;
        c += SAMPLE_MAIN(uv - off * 2.0).rgb * w2;
        c += SAMPLE_MAIN(uv + off * 3.0).rgb * w3;
        c += SAMPLE_MAIN(uv - off * 3.0).rgb * w3;
        c += SAMPLE_MAIN(uv + off * 4.0).rgb * w4;
        c += SAMPLE_MAIN(uv - off * 4.0).rgb * w4;
        return c;
      }

      // Vertical blur
      float3 BlurY(float2 uv, float2 res, float radiusPx)
      {
        float2 off = float2(0.0, radiusPx / res.y);
        float3 c = SAMPLE_MAIN(uv).rgb * w0;
        c += SAMPLE_MAIN(uv + off * 1.0).rgb * w1;
        c += SAMPLE_MAIN(uv - off * 1.0).rgb * w1;
        c += SAMPLE_MAIN(uv + off * 2.0).rgb * w2;
        c += SAMPLE_MAIN(uv - off * 2.0).rgb * w2;
        c += SAMPLE_MAIN(uv + off * 3.0).rgb * w3;
        c += SAMPLE_MAIN(uv - off * 3.0).rgb * w3;
        c += SAMPLE_MAIN(uv + off * 4.0).rgb * w4;
        c += SAMPLE_MAIN(uv - off * 4.0).rgb * w4;
        return c;
      }

      // Apply exposure value
      inline float3 ApplyEV(float3 c, float ev)
      {
        return clamp(c * exp2(ev), 0.0, 0.73);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);

        float2 res = float2(1.0 / TEXEL_SIZE.x, 1.0 / TEXEL_SIZE.y);
        float2 coord = uv * res;
        
        float3 bg = color.rgb;
        #if USE_BLUR
        // Blur background
        float3 bgX = BlurX(uv, res, _BlurPx);
        float3 bgY = BlurY(uv, res, _BlurPx);
        bg = 0.5 * (bgX + bgY);
        bg = ApplyEV(bg, _EV);
        #endif
        bg *= _BackgroundColor;
        bg = ColorBlend(_BackgroundBlend, color.rgb, bg);

        // Calculate dot grid
        float2 cellPx = float2(_Cell, _Cell);
        float2 cellIdx = floor(coord / cellPx);
        float2 centerPx = (cellIdx + 0.5) * cellPx;
        float2 toC = coord - centerPx;
        
        // Shape variation: interpolate between circular and square distance
        float dCircular = length(toC);
        float dSquare = max(abs(toC.x), abs(toC.y));
        float d = lerp(dSquare, dCircular, _BubbleRoundness);
        
        float rad = max(0.1, (_Cell - _Spacing) * 0.5);

        // Sample base color at dot center
        float3 pixel = SAMPLE_MAIN(centerPx / res).rgb;

        // Anti-aliased disc mask
        float aa = fwidth(d);
        float discMask = 1.0 - smoothstep(rad, rad + aa, d);

        // Bevel calculation
        float bw = rad * _BevelK;
        float innerR = max(rad - bw, 0.0);
        float s = clamp((d - innerR) / max(bw, 1e-5), 0.0, 1.0);
        
        // Calculate direction vector based on shape (blend between circular and square gradients)
        float2 rdirCircular = (dCircular > 0.0) ? toC / dCircular : float2(1.0, 0.0);
        
        // Square gradient: points toward the nearest edge
        float2 absToC = abs(toC);
        float2 rdirSquare;
        float maxAxis = max(absToC.x, absToC.y);
        if (maxAxis > 0.0)
        {
          // Normalize based on the dominant axis
          rdirSquare = (absToC.x > absToC.y) ? float2(sign(toC.x), 0.0) : float2(0.0, sign(toC.y));
        }
        else
          rdirSquare = float2(1.0, 0.0);
        
        // Blend direction vectors based on roundness
        float2 rdir = lerp(rdirSquare, rdirCircular, _BubbleRoundness);
        
        float ang = s * 1.57079632679; // PI/2
        float3 nBevel = normalize(float3(rdir * sin(ang), cos(ang)));
        float3 nFlat = float3(0.0, 0.0, 1.0);
        float3 n = lerp(nFlat, nBevel, step(innerR, d));

        // Lighting
        float angleRad = _LightAngle * 0.0174532925; // Convert degrees to radians
        float elevationRad = _LightElevation * 0.0174532925; // Convert degrees to radians
        float cosAngle = cos(angleRad);
        float sinAngle = sin(angleRad);
        float cosElevation = cos(elevationRad);
        float sinElevation = sin(elevationRad);
        // Calculate 3D light direction: azimuth controls XY rotation, elevation controls Z component
        float3 L = normalize(float3(cosAngle * cosElevation, sinAngle * cosElevation, sinElevation));
        float3 V = float3(0.0, 0.0, 1.0);
        float3 H = normalize(L + V);
        float lambert = max(dot(n, L), 0.0);
        float spec = pow(max(dot(n, H), 0.0), _SpecPwr);

        // Rim darkening
        float rimDark = lerp(1.2, 1.0, s);
        pixel = pixel * (0.35 + 0.65 * lambert) * rimDark + (_LightColor * spec * _SpecInt);

        pixel = ColorBlend(_BubbleColorBlend, bg, pixel * _BubbleColor);

        // Combine background and bubbles
        pixel = lerp(bg, pixel, discMask);

        // Apply color adjustments to final result
        pixel = ColorAdjust(pixel, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        return half4(lerp(color.rgb, pixel, _Intensity), color.a);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
