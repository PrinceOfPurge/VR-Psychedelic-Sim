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
Shader "Hidden/Fronkon Games/Weird/Extruder URP"
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
      Name "Fronkon Games Weird Extruder Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma shader_feature_local SHADOWS
      #pragma shader_feature_local AMBIENT_OCCLUSION
      #pragma shader_feature_local GRAYSCALE

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      float _GridScale;
      float _DepthScale;
      float _DepthRemapMin;
      float _DepthRemapMax;
      float _LuminosityRemapMin;
      float _LuminosityRemapMax;
      float _MaxRayDistance;
      float _StepMultiplier;
      float _ShadowSoftness;
      int _RaymarchingSteps;
      float _CameraDistance;
      float3 _LightPosition;
      float4 _LightColor;
      float3 _SpecularColor;
      float _FresnelIntensity;
      float2 _Rotation;
      int _ShadowIterations;
      int _AmbientOcclusionIterations;
      int _ColorBlend;
      float3 _FloorColor;
      int _FloorColorBlend;
      
      // Global variables for scene
      float gObjID;
      float2 gID;
      
      // Standard 2D rotation formula
      inline float2x2 Rot2(float a)
      {
        float c = cos(a);
        float s = sin(a);

        return float2x2(c, -s, s, c);
      }
      
      // Hash function (IQ's vec2 to float hash)
      inline float Hash21(float2 p)
      {
        return frac(sin(dot(p, float2(27.609, 57.583))) * 43758.5453);
      }

      // Get texture with aspect ratio correction
      float3 GetTex(float2 p)
      {
        // Stretch to fill window
        p *= float2(_ScreenParams.y / _ScreenParams.x, 1.0);
        float3 tx = SAMPLE_MAIN(frac(p / 2.0 - 0.5)).xyz;
        return tx * tx; // Rough sRGB to linear
      }
      
      // Get texture with explicit LOD (for use in loops)
      float3 GetTexLOD(float2 p)
      {
        // Stretch to fill window
        p *= float2(_ScreenParams.y / _ScreenParams.x, 1.0);
        float3 tx = SAMPLE_MAIN_LOD(frac(p / 2.0 - 0.5)).xyz;
        return tx * tx; // Rough sRGB to linear
      }
      
      // Height map value with explicit LOD (for use in loops)
      inline float HeightMapLOD(float2 p)
      {
        #ifdef GRAYSCALE
          // Use grayscale luminance
          float luminosity = dot(GetTexLOD(p), float3(0.299, 0.587, 0.114));
          
          // Remap luminosity from [min, max] to [0, 1]
          luminosity = saturate((luminosity - _LuminosityRemapMin) / max(_LuminosityRemapMax - _LuminosityRemapMin, 0.0001));
          
          return luminosity;
        #else
          // Use depth buffer - stretch to match aspect ratio
          float2 depthUV = p * float2(_ScreenParams.y / _ScreenParams.x, 1.0);
          depthUV = frac(depthUV / 2.0 - 0.5);
          float depth = SampleLinear01DepthLOD(depthUV);
          
          // Remap depth from [min, max] to [0, 1]
          depth = 1.0 - saturate((depth - _DepthRemapMin) / max(_DepthRemapMax - _DepthRemapMin, 0.0001));
          
          // Apply depth scale to accentuate height differences
          return saturate(depth * _DepthScale);
        #endif
      }
      
      // Extrusion formula (https://iquilezles.org/)
      inline float OpExtrusion(float sdf, float pz, float h)
      {
        float2 w = float2(sdf, abs(pz) - h);

        return min(max(w.x, w.y), 0.0) + length(max(w, 0.0));
      }
      
      // Unsigned box formula with smoothing (https://iquilezles.org/)
      inline float SBoxS(float2 p, float2 b, float sf)
      {
        return length(max(abs(p) - b + sf, 0.0)) - sf;
      }
      
      // Extruded block grid with subdivision
      float4 Blocks(float3 q3)
      {
        const float scale = _GridScale;
        const float2 l = float2(scale, scale);
        const float2 s = l * 2.0;
        
        float d = 1e5;
        float2 p, ip;
        float2 id = float2(0, 0);
        float2 cntr = float2(0, 0);
        
        float boxID = 0.0;
        
        // Four block corner positions
        float2 ps4[4];
        ps4[0] = float2(-l.x, l.y);
        ps4[1] = l;
        ps4[2] = -l;
        ps4[3] = float2(l.x, -l.y);
        [unroll]
        for (int i = 0; i < 4; i++)
        {
          cntr = ps4[i] / 2.0;
          p = q3.xy - cntr;
          ip = floor(p / s) + 0.5;
          p -= ip * s;
          
          float2 idi = ip * s + cntr;
          
          // Main block height
          float h = HeightMapLOD(idi);
          
          // Check for subdivision
          float4 h4;
          int sub = 0;
          [unroll]
          for (int j = 0; j < 4; ++j)
          {
            h4[j] = HeightMapLOD(idi + ps4[j] / 4.0);
            if (abs(h4[j] - h) > 1.0 / 15.0)
              sub = 1;
          }
          
          // Quantize heights
          h = floor(h * 15.999) / 15.0 * 0.15;
          h4 = floor(h4 * 15.999) / 15.0 * 0.15;
          
          if (sub == 1)
          {
            // Four smaller extruded blocks
            float4 d4, di4;
            [unroll]
            for (int j = 0; j < 4; j++)
            {
              d4[j] = SBoxS(p - ps4[j] / 4.0, l / 4.0 - 0.05 * scale, 0.005);
              di4[j] = OpExtrusion(d4[j], q3.z + h4[j], h4[j]);
              
              if (di4[j] < d)
              {
                d = di4[j];
                id = idi + ps4[j] / 4.0;
              }
            }
          }
          else
          {
            // One larger extruded block
            float di2D = SBoxS(p, l / 2.0 - 0.05 * scale, 0.015);
            float di = OpExtrusion(di2D, q3.z + h, h);
            
            if (di < d)
            {
              d = di;
              id = idi;
            }
          }
        }
        
        return float4(d, id, boxID);
      }
      
      // Distance function
      float Map(float3 p)
      {
        // Floor
        float fl = -p.z + 0.1;
        
        // Extruded blocks
        float4 d4 = Blocks(p);
        gID = d4.yz;
        
        gObjID = fl < d4.x ? 1.0 : 0.0;
        
        return min(fl, d4.x);
      }
      
      // Raymarcher
      float Trace(float3 ro, float3 rd)
      {
        float t = 0.0;
        float d;
        
        UNITY_LOOP
        for (int i = 0; i < _RaymarchingSteps; i++)
        {
          d = Map(ro + rd * t);
          if (abs(d) < 0.001 || t > _MaxRayDistance)
            break;
          t += d * _StepMultiplier;
        }
        
        return min(t, _MaxRayDistance);
      }
      
      // Normal calculation
      inline float3 GetNormal(float3 p, float t)
      {
        const float2 e = float2(0.001, 0.0);
        return normalize(float3(
          Map(p + e.xyy) - Map(p - e.xyy),
          Map(p + e.yxy) - Map(p - e.yxy),
          Map(p + e.yyx) - Map(p - e.yyx)
        ));
      }
      
      // Soft shadows
      float SoftShadow(float3 ro, float3 lp, float3 n, float k)
      {
        ro += n * 0.0015;
        float3 rd = lp - ro;
        
        float shade = 1.0;
        float t = 0.0001;
        float end = max(length(rd), 0.0001);
        rd /= end;
        
        UNITY_LOOP
        for (int i = 0; i < _ShadowIterations; i++)
        {
          float d = Map(ro + rd * t);
          shade = min(shade, k * d / t);
          t += clamp(d, 0.01, 0.25);
          
          if (d < 0.0 || t > end)
            break;
        }
        
        return max(shade, 0.0);
      }
      
      // Ambient occlusion
      float CalcAO(float3 p, float3 n)
      {
        float sca = 3.0;
        float occ = 0.0;

        UNITY_LOOP
        for (int i = 0; i < _AmbientOcclusionIterations; i++)
        {
          float hr = float(i + 1) * 0.15 / 5.0;
          float d = Map(p + n * hr);
          occ += (hr - d) * sca;
          sca *= 0.7;
        }
        
        return clamp(1.0 - occ, 0.0, 1.0);
      }
      
      // Main extruder effect
      half4 ExtruderEffect(float2 fragCoord, half4 originalColor)
      {
        // Screen coordinates (centered)
        float2 uv = (fragCoord * _ScreenParams.xy - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        
        // Camera setup
        float3 lk = float3(0, 0, 0);
        float3 ro = float3(0, 0, _CameraDistance);
        
        // Light position (relative to camera)
        float3 lp = ro + _LightPosition;
        
        // Ray direction
        float FOV = 1.0;
        float3 fwd = normalize(lk - ro);
        float3 rgt = normalize(float3(fwd.z, 0.0, -fwd.x));
        float3 up = cross(fwd, rgt);
        float3 rd = normalize(fwd + FOV * uv.x * rgt + FOV * uv.y * up);

        // Rotation
        float2 a = sin(float2(1.5707963, 0.0) - _Rotation.x); 
        float2x2 rM = float2x2(a, -a.y, a.x);
        rd.xz = mul(rd.xz, rM); 
        a = sin(float2(1.5707963, 0.0) - _Rotation.y); 
        rM = float2x2(a, -a.y, a.x);
        rd.yz = mul(rd.yz, rM);
        
        // Raymarch
        float t = Trace(ro, rd);
        
        // Save IDs
        float2 svGID = gID;
        float svObjID = gObjID;
        
        // Initial color
        float3 col = float3(0, 0, 0);
        
        // If hit surface
        if (t < _MaxRayDistance)
        {
          float3 sp = ro + rd * t;
          float3 sn = GetNormal(sp, t);
          
          // Get texture color
          float3 texCol;
          
          if (svObjID < 0.5)
          {
            // Extruded blocks - color from saved ID
            float3 tx = GetTex(svGID);
            texCol = ColorAdjust(tx, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);
            texCol = smoothstep(0.0, 1.0, texCol);
          }
          else
          {
            // Floor - use floor color with blend mode
            texCol = ColorBlend(_FloorColorBlend, originalColor.rgb, _FloorColor.rgb);
          }
          
          // Lighting
          float3 ld = lp - sp;
          float lDist = max(length(ld), 0.001);
          ld /= lDist;
          
          // Shadows and AO
          #ifdef SHADOWS
          float sh = SoftShadow(sp, lp, sn, _ShadowSoftness);
          #else
          float sh = 1.0;
          #endif

          #ifdef AMBIENT_OCCLUSION
          float ao = CalcAO(sp, sn);
          sh = min(sh + ao * 0.25, 1.0);
          #else
          float ao = 1.0;
          #endif
          
          // Attenuation
          float atten = 1.0 / (1.0 + lDist * 0.05);
          
          // Diffuse
          float diff = max(dot(sn, ld), 0.0);
          
          // Specular
          float spec = pow(max(dot(reflect(ld, sn), rd), 0.0), 10.0);
          
          // Fresnel
          float fre = pow(clamp(dot(sn, rd) + 1.0, 0.0, 1.0), 2.0);
          
          // Combine
          col = texCol * (diff + ao * _LightColor.a + _LightColor.rgb * diff * fre * _FresnelIntensity + _SpecularColor * spec * 2.0);
          col *= ao * sh * atten;
        }
        
        // Gamma correction
        col = sqrt(max(col, 0.0));
        
        return half4(col, originalColor.a);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);

        half4 pixel = ExtruderEffect(uv, color);

        pixel.rgb = ColorBlend(_ColorBlend, color.rgb, pixel.rgb);

#if 0
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif

        return half4(lerp(color.rgb, pixel.rgb, _Intensity), color.a);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
