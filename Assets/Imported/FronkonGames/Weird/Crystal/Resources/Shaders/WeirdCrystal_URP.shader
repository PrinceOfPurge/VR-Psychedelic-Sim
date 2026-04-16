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
Shader "Hidden/Fronkon Games/Weird/Crystal URP"
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
      Name "Fronkon Games Weird Crystal Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile ___ DEBUG_NORMALS
      #pragma multi_compile ___ USE_CRYSTAL
      #pragma multi_compile ___ USE_REFRACTION
      #pragma multi_compile ___ USE_LIGHTS

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      float _CrystalIntensity;
      int _CrystalColorBlend;
      float _CrystalGain;
      float _CrystalSpeed;
      float3 _CrystalColor;
      float _CrystalScale;
      float _CrystalPower;
      float _CrystalRotation0;
      float _CrystalRotation1;
      float _CrystalReflection;
      float _CrystalRefraction;

      float _LightsIntensity;
      int _LightsColorBlend;
      float _LightsSpeed;
      int _LightsIterations;
      float3 _LightsColorOffset;
      float _LightsComplexity;
      float _LightsDistortion;
      float _LightsSpread;
      float _LightsRotationSpeed;
      float _LightsTurbulence;
      float _LightsDetail;
      float _LightsWarp;
      float _LightsBrightness;
      float _LightsContrast;
      float _LightsPower;

      #define D2R (PI / 180.0)
      #define TWOPI 6.283184

      inline float ABS1d(in float x)   { return abs(frac(x) - 0.5); }
      inline float2 ABS2d(in float2 v) { return abs(frac(v) - 0.5); }
      inline float Cos1d(in float p) { return cos(p * TWOPI) * 0.25 + 0.25; }
      inline float Sin1d(in float p) { return sin(p * TWOPI) * 0.25 + 0.25; }

      inline float2x2 Rot(in float a)
      {
        float c = cos(a);
        float s = sin(a);
        return float2x2(c, -s, s, c);
      }

      inline float2 Quadrant(in float2 p)
      {
        float angle = round(atan2(p.x, p.y) * 4.0) / 4.0;
        p = mul(Rot(angle), p);
        p *= 2.0;
        return p;
      }

      inline float2x2 RotMat(in float r) { float c = cos(r); float s = sin(r); return float2x2(c, -s, s, c); }

      inline float EaseFade(float x)
      {
        return 1.0 - (2.0 * x - 1.0) * (2.0 * x - 1.0) * (2.0 * x - 1.0) * (2.0 * x - 1.0);
      }

      inline float2 GetPos(float t, float life, float offset, float lo)
      {
        return float2(cos(offset+floor((t-lo)/life)*life)*_ScreenParams.x/2.,
                      sin(2.0*offset+floor((t-lo)/life)*life)*_ScreenParams.y/2.);
      }

      inline float HoleFade(float t, float life, float lo)
      {
        return EaseFade(mod(t - lo, life) / life);
      }      

      float3 CrystalNoise(float2 pos)
      {
        const float time = _EffectTime.y * _CrystalSpeed;
        float2 q = 0.0;
        float result = 0.0;
        float2 aPos = ABS2d(pos) * 0.5;

        for (float i = 0.0; i < 15.0; ++i)
        {
          pos = mul(RotMat(D2R * _CrystalRotation0), pos);
          float t = (sin(time) * 0.5 + 0.5) * 0.2 + time * 0.8;
          q = pos * _CrystalScale + t;
          q = pos * _CrystalScale + aPos + t;
          q = (float2)cos(q);

          result += Sin1d(dot(q, (float2)0.3)) * _CrystalGain;

          _CrystalScale *= 1.07;
          aPos += cos(smoothstep(0.0, 0.15, q));
          aPos = mul(RotMat(D2R * _CrystalRotation1), aPos);
          aPos *= 1.232;
        }

        return clamp(_CrystalColor / ABS1d(dot(q, float2(-0.240, 0.0))) * 0.5 / pow(abs(result), _CrystalPower),
                     (float3)0.0,
                     (float3)1.0);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const float2 coord = uv * _ScreenParams.xy;
        const half4 color = SAMPLE_MAIN(uv);

        float4 pixel = color;

        #if USE_CRYSTAL
          float center = length(-1.0 + 2.0 * coord / _ScreenParams.xy);
          
          float2 uvCrystal = uv;
          uvCrystal.x = ((uvCrystal.x - 0.5) * (_ScreenParams.x / _ScreenParams.y)) + 0.5;
          float stMask = step(0.0, uvCrystal.x * (1.0 - uvCrystal.x));
          uvCrystal *= 3.0;

          float2 pix = 1.0 / _ScreenParams.xy;
          float3 crystal = 0.0;
          [unroll]
          for (float i = 0.0; i < 1.0; ++i)
          {
            [unroll]
            for (float j = 0.0; j < 1.0; ++j)
              crystal += CrystalNoise(uvCrystal + pix * float2(i + 0.5, j + 0.5));
          }

          #if USE_REFRACTION
            float2 refraction = float2(crystal.r, crystal.r);
            refraction *= _CrystalRefraction * 0.1;
            
            float scaleAmount = 1.0 + (crystal.r - 0.5) * _CrystalRefraction * 0.2;
            float2 centeredUV = (uv - 0.5) * scaleAmount + 0.5;
            
            float2 refractedUV = saturate(centeredUV + refraction);
            
            half3 reflectedColor = SAMPLE_MAIN(refractedUV).rgb;
            
            crystal = lerp(crystal, reflectedColor, _CrystalReflection);
            crystal = pow(abs(crystal), 1.5);
          #endif

          pixel.rgb = ColorBlend(_CrystalColorBlend, color.rgb, crystal * _CrystalIntensity);
        #endif

        #if USE_LIGHTS
          float2 v = _ScreenParams.xy;
          float2 u = 0.1 * (coord + coord - v) / v.y;
          
          float3 z = _LightsColorOffset;
          float3 lights = 0.0;
          
          float a = 0.5;
          float t = _EffectTime.y * _LightsSpeed;
          for (float l = 0.0; l < _LightsIterations; l += 1.0)
          {
            t += 1.0;
            v = cos(t - _LightsDistortion * u * pow(abs(a + _LightsComplexity), l + 1.0)) - _LightsSpread * u;
          
            float angle = l + _LightsRotationSpeed * t;
            float2x2 rotMat = float2x2(
                cos(angle),         cos(angle + 11.0),
                cos(angle + 33.0),  cos(angle + 0.0));
            u = mul(rotMat, u);

            float uDotU = dot(u, u);
            float tanhTerm = tanh(_LightsTurbulence * uDotU * cos(100.0 * u.yx + t)) / 200.0;
            float linearTerm = 0.2 * a * u;
            float cosTerm = cos(4.0 / exp(dot(lights, lights) / 100.0) + t) / 300.0;

            u += tanhTerm + linearTerm + float2(cosTerm, cosTerm);
          
            float2 sinArg = _LightsDetail * u / (0.5 - dot(u, u)) - _LightsWarp * u.yx + t;
            float lenTerm = length((1.0 + (l + 1.0) * dot(v, v)) * sin(sinArg));
            
            lights += (1.0 + cos(z + t)) / lenTerm;          
          }

          lights = _LightsBrightness / (min(lights, _LightsContrast) + 164.0 / lights) - dot(u, u) / 250.0;
          lights = pow(abs(lights), _LightsPower);

          pixel.rgb = ColorBlend(_LightsColorBlend, pixel.rgb, lights * _LightsIntensity);
        #endif

        // Apply color adjustments to final result
        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

#if 0
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }    
  }
  
  FallBack "Diffuse"
}
