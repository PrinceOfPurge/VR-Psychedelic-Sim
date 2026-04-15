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
Shader "Hidden/Fronkon Games/Weird/Fire Tunnel URP"
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
      Name "Fronkon Games Weird Fire Tunnel Pass"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment WeirdFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile ___ DEBUG_NORMALS

      #include "Weird.hlsl"
      #include "ColorBlend.hlsl"

      float2 _Center;
      float _Speed;
      float _TunnelRadius;
      float _Turbulence;
      float _RotationSpeed;
      int _RaymarchSteps;
      float _NoiseScale;
      float _FireIntensity;
      float3 _FireColor;
      int _ColorBlend;

      // 2D rotation matrix
      float2x2 Rotate2D(float angle)
      {
        float s = sin(angle);
        float c = cos(angle);
        return float2x2(c, -s, s, c);
      }

      half4 WeirdFrag(WeirdVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const half4 color = SAMPLE_MAIN(uv);

        float4 pixel = 0.0;

        float i = 0.0;
        float d = 0.0;
        float s = 0.0;
        float n = 0.0;
        float t = _EffectTime.y * _Speed;

        // Get screen resolution
        float2 coord = (uv - _Center) * _ScreenParams.xy;

        // Convert to normalized coordinates centered at screen center
        float2 u = (coord - _ScreenParams.xy * 0.5) / _ScreenParams.y;
        
        // Raymarching loop with customizable steps
        UNITY_LOOP
        for (i = 0; i < _RaymarchSteps; i++)
        {
          // Ray position: ro + rd * d
          float3 p = float3(u * d, d + t * 4.0);
          
          // Turbulence - add cosine waves for fluid motion
          p += cos(p.z + t + p.yzx * 0.5) * _Turbulence;

          // Tunnel radius
          s = _TunnelRadius + 1.0 * _TunnelRadius - length(p.xy);
          
          // Rotate the tunnel
          p.xy = mul(Rotate2D(t * _RotationSpeed), p.xy);
          
          // Noise loop - multi-octave noise
          for (n = 1.6; n < 32.0; n += n)
          {
            // Subtract noise from tunnel distance
            float3 sinP = sin(p.z + t + p * n);
            s -= abs(dot(sinP, float3(_NoiseScale, _NoiseScale, _NoiseScale))) / n;
          }
          
          // Accumulate distance
          d += s = 0.01 + abs(s) * 0.1;
          
          // Accumulate grayscale color
          pixel += 1.0 / s;
        }
        
        // Apply color grading for fire effect
        pixel *= pixel / d / 2e6 * _FireIntensity;
        pixel.rgb = saturate(pixel.rgb);

        // Apply tanh for tone mapping with custom fire colors
        pixel = tanh(float4(_FireColor, 1.0) * pixel);

        // Apply color adjustments to fire effect
        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        // Apply color blend operation
        pixel.rgb = lerp(color.rgb, ColorBlend(_ColorBlend, color.rgb, pixel.rgb), (pixel.r + pixel.g + pixel.b) / 3.0);
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
