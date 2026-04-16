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
Shader "Hidden/Fronkon Games/Weird/Pinch URP"
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
      Name "Fronkon Games Weird Pinch Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      float2 _Start;
      float2 _End;
      float _StartRadius;
      float _EndRadius;
      float _RaymarchSteps;
      float _MaxDistance;
      float _Roundness;
      float3 _LightDirection;
      float _LightIntensity;

      // Smooth minimum function
      inline float smin(float d1, float d2, float k)
      {
        float h = max(k - abs(d1 - d2), 0.0) / k;

        return min(d1, d2) - h * h * k * 0.25;
      }

      // Signed Distance Function - returns distance and stores distorted domain in g
      float map(float3 p, inout float3 g)
      {
        float2 center = ((_Start * _ScreenParams.xy) - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        float2 mouse = ((_End * _ScreenParams.xy) - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        
        float2 offset = mouse - center;
        if (length(offset) < 0.001)
          offset = float2(0.0, 0.0);

        p.xy -= p.z * offset;
        g = p;
        p.xy -= center;

        float plane = p.z;

        float zNormalized = saturate(p.z);
        float coneRadius = lerp(_StartRadius, _EndRadius, zNormalized);
        
        p.z -= 1.0;
        float coneTaper = lerp(10.0, 2.0, _Roundness);
        float cone = length(p.xy) - coneRadius + p.z / coneTaper;

        p.z *= 3.0;
        float smoothBlend = lerp(0.1, 1.0, _Roundness);
        float result = smin(plane, cone, smoothBlend);

        return result / 1.73205080757; // sqrt(3.0)
      }

      // Calculate normal using finite differences
      float3 CalculateNormal(float3 p)
      {
        const float eps = 0.001;
        float3 gTemp;
        float3 n;

        n.x = map(float3(p.x + eps, p.y, p.z), gTemp) - map(float3(p.x - eps, p.y, p.z), gTemp);
        n.y = map(float3(p.x, p.y + eps, p.z), gTemp) - map(float3(p.x, p.y - eps, p.z), gTemp);
        n.z = map(float3(p.x, p.y, p.z + eps), gTemp) - map(float3(p.x, p.y, p.z - eps), gTemp);

        return normalize(n);
      }

      // Raymarch function - returns final position and distorted domain
      void Raymarch(inout float3 p, float3 rd, out float3 g)
      {
        float dd = 0.0;
        float d0 = _MaxDistance;
        g = float3(0.0, 0.0, 0.0);
        
        UNITY_LOOP
        for (float i = 0.0; i < _RaymarchSteps; i += 1.0)
        {
          float d = map(p, g);
          if (d < 1e-4 || dd > d0)
          {
            if (d < 1e-4)
              map(p, g);
            break;
          }
          p += rd * d;
          dd += d;
        }
      }

      // Render function
      float3 Render(float3 p, float3 rd, float2 originalUV)
      {
        float2 center = ((_Start * _ScreenParams.xy) - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        float2 mouse = ((_End * _ScreenParams.xy) - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        float2 offset = mouse - center;
        
        if (length(offset) < 0.001)
        {
          float3 col = SAMPLE_MAIN(originalUV).rgb;
          return col;
        }

        float3 g;
        float3 hitPos = p;
        Raymarch(hitPos, rd, g);

        g.xy *= float2(_ScreenParams.y / _ScreenParams.x, 1.0);
        float2 texUV = g.xy + 0.5;
        float3 col = SAMPLE_MAIN(texUV).rgb;
        
        float3 normal = CalculateNormal(hitPos);
        float3 lightDir = normalize(_LightDirection);
        float lambert = max(dot(normal, lightDir), 0.0);
        
        return lerp(col, col * lambert, _LightIntensity);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);
        const float2 coord = uv * _ScreenParams.xy;
        
        float2 uvCentered = (coord - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        float3 ro = float3(uvCentered.x, uvCentered.y, _MaxDistance);
        
        half4 pixel = half4(Render(ro, float3(0.0, 0.0, -1.0), uv), 1.0);

        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);
        
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }

  FallBack "Diffuse"
}

