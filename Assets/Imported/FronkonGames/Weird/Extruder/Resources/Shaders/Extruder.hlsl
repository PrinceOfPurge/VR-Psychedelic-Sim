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
#pragma once

// Shader parameters
float _GridScale;
float _MaxRayDistance;
float _StepMultiplier;
float _ShadowSoftness;
float _CameraDistance;
float3 _LightPosition;

// Global variables for scene
float gObjID;
float2 gID;

// Standard 2D rotation formula
float2x2 Rot2(float a)
{
  float c = cos(a);
  float s = sin(a);
  return float2x2(c, -s, s, c);
}

// Hash function (IQ's vec2 to float hash)
float Hash21(float2 p)
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

// Height map value (greyscale)
float HeightMap(float2 p)
{
  return dot(GetTex(p), float3(0.299, 0.587, 0.114));
}

// IQ's extrusion formula
float OpExtrusion(float sdf, float pz, float h)
{
  float2 w = float2(sdf, abs(pz) - h);
  return min(max(w.x, w.y), 0.0) + length(max(w, 0.0));
}

// IQ's unsigned box formula with smoothing
float SBoxS(float2 p, float2 b, float sf)
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
  
  for (int i = 0; i < 4; i++)
  {
    cntr = ps4[i] / 2.0;
    p = q3.xy - cntr;
    ip = floor(p / s) + 0.5;
    p -= ip * s;
    
    float2 idi = ip * s + cntr;
    
    // Main block height
    float h = HeightMap(idi);
    
    // Check for subdivision
    float4 h4;
    int sub = 0;
    for (int j = 0; j < 4; j++)
    {
      h4[j] = HeightMap(idi + ps4[j] / 4.0);
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
  
  for (int i = 0; i < 64; i++)
  {
    d = Map(ro + rd * t);
    if (abs(d) < 0.001 || t > _MaxRayDistance)
      break;
    t += d * _StepMultiplier;
  }
  
  return min(t, _MaxRayDistance);
}

// Normal calculation
float3 GetNormal(float3 p, float t)
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
  const int maxIter = 24;
  ro += n * 0.0015;
  float3 rd = lp - ro;
  
  float shade = 1.0;
  float t = 0.0;
  float end = max(length(rd), 0.0001);
  rd /= end;
  
  for (int i = 0; i < maxIter; i++)
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
  
  for (int i = 0; i < 5; i++)
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
      
      #ifdef GRAYSCALE
      texCol = float3(1, 1, 1) * dot(tx, float3(0.299, 0.587, 0.114));
      #else
      texCol = tx;
      #endif
      
      texCol = smoothstep(0.0, 1.0, texCol);
    }
    else
    {
      // Floor - dark background
      texCol = float3(0, 0, 0);
    }
    
    // Lighting
    float3 ld = lp - sp;
    float lDist = max(length(ld), 0.001);
    ld /= lDist;
    
    // Shadows and AO
    float sh = SoftShadow(sp, lp, sn, _ShadowSoftness);
    float ao = CalcAO(sp, sn);
    sh = min(sh + ao * 0.25, 1.0);
    
    // Attenuation
    float atten = 1.0 / (1.0 + lDist * 0.05);
    
    // Diffuse
    float diff = max(dot(sn, ld), 0.0);
    
    // Specular
    float spec = pow(max(dot(reflect(ld, sn), rd), 0.0), 16.0);
    
    // Fresnel
    float fre = pow(clamp(dot(sn, rd) + 1.0, 0.0, 1.0), 2.0);
    
    // Combine
    col = texCol * (diff + ao * 0.3 + float3(0.25, 0.5, 1.0) * diff * fre * 16.0 + float3(1.0, 0.5, 0.2) * spec * 2.0);
    col *= ao * sh * atten;
  }
  
  // Gamma correction
  col = sqrt(max(col, 0.0));
  
  return half4(col, originalColor.a);
}

