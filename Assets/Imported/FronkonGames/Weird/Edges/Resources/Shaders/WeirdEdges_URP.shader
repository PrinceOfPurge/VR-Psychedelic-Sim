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
Shader "Hidden/Fronkon Games/Weird/Edges"
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

    HLSLINCLUDE
    #include "Weird.hlsl"
    #include "ColorBlend.hlsl"

    float _Edges;
    float _EdgeWidth;
    float _EdgeThreshold;
    float _EdgeSoftening;
    float _EdgeDetectionMethod;
    int _EdgesBlend;
    float3 _EdgesColor;
    float3 _BackgroundColor;
    int _BackgroundBlend;
    float3 _DepthEdgeNearColor;
    float3 _DepthEdgeFarColor;
    float _DepthEdgeNear;
    float _DepthEdgeFar;

    // Function to sample edge weight at an offset (for glow calculation)
    inline float SampleEdgeWeight(float2 uv, float2 borderSize)
    {
      float borderMask = smoothstep(0.0, borderSize.x, uv.x) * smoothstep(0.0, borderSize.x, 1.0 - uv.x) *
                         smoothstep(0.0, borderSize.y, uv.y) * smoothstep(0.0, borderSize.y, 1.0 - uv.y);

      float c = SampleLinear01DepthLOD(uv);
      float4 pix = float4(TEXEL_SIZE.xy, -TEXEL_SIZE.xy) * _EdgeWidth;
      float4 depthEven = float4(SampleLinear01DepthLOD(uv + float2(0.0, pix.w)),
                                SampleLinear01DepthLOD(uv + float2(0.0, pix.y)),
                                SampleLinear01DepthLOD(uv + float2(pix.x, 0.0)),
                                SampleLinear01DepthLOD(uv + float2(pix.z, 0.0)));
      float4 depthOdd  = float4(SampleLinear01DepthLOD(uv + float2(pix.x, pix.w)),
                                SampleLinear01DepthLOD(uv + float2(pix.z, pix.y)),
                                SampleLinear01DepthLOD(uv + float2(pix.x, pix.y)),
                                SampleLinear01DepthLOD(uv + float2(pix.z, pix.w)));

      float2 mind = float2(min4(depthEven), min4(depthOdd));
      float2 maxd = float2(max4(depthEven), max4(depthOdd));
      float span = max2(maxd) - min2(mind) + 0.00001;
      c /= span;
      depthEven /= span;
      depthOdd /= span;

      float4 diffsEven = abs(depthEven - c);
      float4 diffsOdd = abs(depthOdd - c);
      float2 retVal = float2(max(abs(diffsEven.x - diffsEven.y), abs(diffsEven.z - diffsEven.w)),
                            max(abs(diffsOdd.x - diffsOdd.y), abs(diffsOdd.z - diffsOdd.w)));

      float lineWeight = max2(retVal) * borderMask;
      return step(_EdgeThreshold, lineWeight);
    }

    half4 EdgesFrag(WeirdVaryings input) : SV_Target
    {
      UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
      const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
      const float4 color = SAMPLE_MAIN(uv);

      float3 pixel = color.rgb;

      // Calculate border mask to suppress false edges at screen borders
      float2 borderSize = TEXEL_SIZE.xy * _EdgeWidth * 4.0;
      float borderMask = smoothstep(0.0, borderSize.x, uv.x) * smoothstep(0.0, borderSize.x, 1.0 - uv.x) *
                         smoothstep(0.0, borderSize.y, uv.y) * smoothstep(0.0, borderSize.y, 1.0 - uv.y);

      float lineWeight = 0.0;

      float depthClamped = SampleLinear01DepthLOD(uv);
#if defined(_EDGES_IGNORE_SKYBOX)
      if (depthClamped > 0.999)
        return color;
#endif

#if defined(_EDGES_DEPTH_NEIGHBORS)
      // Depth-based edge detection using neighboring pixels (original method)
      float4 pix = float4(TEXEL_SIZE.xy, -TEXEL_SIZE.xy) * _EdgeWidth;

      float4 depthEven = float4(SampleLinear01Depth(uv + float2(0.0, pix.w)),
                                SampleLinear01Depth(uv + float2(0.0, pix.y)),
                                SampleLinear01Depth(uv + float2(pix.x, 0.0)),
                                SampleLinear01Depth(uv + float2(pix.z, 0.0)));

      float4 depthOdd  = float4(SampleLinear01Depth(uv + float2(pix.x, pix.w)),
                                SampleLinear01Depth(uv + float2(pix.z, pix.y)),
                                SampleLinear01Depth(uv + float2(pix.x, pix.y)),
                                SampleLinear01Depth(uv + float2(pix.z, pix.w)));

      // Normalize values
      float2 mind = float2(min4(depthEven), min4(depthOdd));
      float2 maxd = float2(max4(depthEven), max4(depthOdd));
      float span = max2(maxd) - min2(mind) + 0.00001;
      depthClamped /= span;
      depthEven /= span;
      depthOdd /= span;

      // Calculate the distance of the surrounding pixels to the center
      float4 diffsEven = abs(depthEven - depthClamped);
      float4 diffsOdd = abs(depthOdd - depthClamped);

      // Calculate the difference of the (opposing) distances
      float2 retVal = float2(max(abs(diffsEven.x - diffsEven.y), abs(diffsEven.z - diffsEven.w)),
                            max(abs(diffsOdd.x - diffsOdd.y), abs(diffsOdd.z - diffsOdd.w)));

      lineWeight = max2(retVal);

#elif defined(_EDGES_SOBEL)
      // Sobel operator for edge detection
      float2 texelSize = TEXEL_SIZE.xy * _EdgeWidth;

      // Sample depth values in 3x3 kernel
      float depth[9];
      depth[0] = SampleLinear01Depth(uv + float2(-texelSize.x, -texelSize.y));
      depth[1] = SampleLinear01Depth(uv + float2(0.0, -texelSize.y));
      depth[2] = SampleLinear01Depth(uv + float2(texelSize.x, -texelSize.y));
      depth[3] = SampleLinear01Depth(uv + float2(-texelSize.x, 0.0));
      depth[4] = depthClamped;
      depth[5] = SampleLinear01Depth(uv + float2(texelSize.x, 0.0));
      depth[6] = SampleLinear01Depth(uv + float2(-texelSize.x, texelSize.y));
      depth[7] = SampleLinear01Depth(uv + float2(0.0, texelSize.y));
      depth[8] = SampleLinear01Depth(uv + float2(texelSize.x, texelSize.y));

      // Sobel X kernel
      float sobelX = depth[0] * -1.0 + depth[2] * 1.0 +
                     depth[3] * -2.0 + depth[5] * 2.0 +
                     depth[6] * -1.0 + depth[8] * 1.0;

      // Sobel Y kernel
      float sobelY = depth[0] * -1.0 + depth[1] * -2.0 + depth[2] * -1.0 +
                     depth[6] * 1.0 + depth[7] * 2.0 + depth[8] * 1.0;

      lineWeight = sqrt(sobelX * sobelX + sobelY * sobelY);

#elif defined(_EDGES_PREWITT)
      // Prewitt operator for edge detection
      float2 texelSize = TEXEL_SIZE.xy * _EdgeWidth;

      // Sample depth values in 3x3 kernel
      float depth[9];
      depth[0] = SampleLinear01Depth(uv + float2(-texelSize.x, -texelSize.y));
      depth[1] = SampleLinear01Depth(uv + float2(0.0, -texelSize.y));
      depth[2] = SampleLinear01Depth(uv + float2(texelSize.x, -texelSize.y));
      depth[3] = SampleLinear01Depth(uv + float2(-texelSize.x, 0.0));
      depth[4] = depthClamped;
      depth[5] = SampleLinear01Depth(uv + float2(texelSize.x, 0.0));
      depth[6] = SampleLinear01Depth(uv + float2(-texelSize.x, texelSize.y));
      depth[7] = SampleLinear01Depth(uv + float2(0.0, texelSize.y));
      depth[8] = SampleLinear01Depth(uv + float2(texelSize.x, texelSize.y));

      // Prewitt X kernel
      float prewittX = depth[0] * -1.0 + depth[2] * 1.0 +
                       depth[3] * -1.0 + depth[5] * 1.0 +
                       depth[6] * -1.0 + depth[8] * 1.0;

      // Prewitt Y kernel
      float prewittY = depth[0] * -1.0 + depth[1] * -1.0 + depth[2] * -1.0 +
                       depth[6] * 1.0 + depth[7] * 1.0 + depth[8] * 1.0;

      lineWeight = sqrt(prewittX * prewittX + prewittY * prewittY);

#elif defined(_EDGES_NORMAL_BASED)
      // Normal-based edge detection using surface normals
      // This requires access to normal texture, but we'll approximate using depth gradients

      float2 texelSize = TEXEL_SIZE.xy * _EdgeWidth;

      // Sample depth in cross pattern to estimate surface normal
      float depthC = depthClamped;
      float depthN = SampleLinear01Depth(uv + float2(0.0, texelSize.y));
      float depthS = SampleLinear01Depth(uv + float2(0.0, -texelSize.y));
      float depthE = SampleLinear01Depth(uv + float2(texelSize.x, 0.0));
      float depthW = SampleLinear01Depth(uv + float2(-texelSize.x, 0.0));

      // Estimate normal from depth gradients (simplified)
      float2 gradient;
      gradient.x = (depthE - depthW) * 0.5;
      gradient.y = (depthN - depthS) * 0.5;

      // Edge strength based on normal discontinuity
      lineWeight = length(gradient);

#elif defined(_EDGES_COLOR_BASED)
      // Color-based edge detection in RGB space
      float2 texelSize = TEXEL_SIZE.xy * _EdgeWidth;

      // Sample colors in 3x3 kernel
      float3 kernel[9];
      kernel[0] = SAMPLE_MAIN(uv + float2(-texelSize.x, -texelSize.y)).rgb;
      kernel[1] = SAMPLE_MAIN(uv + float2(0.0, -texelSize.y)).rgb;
      kernel[2] = SAMPLE_MAIN(uv + float2(texelSize.x, -texelSize.y)).rgb;
      kernel[3] = SAMPLE_MAIN(uv + float2(-texelSize.x, 0.0)).rgb;
      kernel[4] = SAMPLE_MAIN(uv).rgb;
      kernel[5] = SAMPLE_MAIN(uv + float2(texelSize.x, 0.0)).rgb;
      kernel[6] = SAMPLE_MAIN(uv + float2(-texelSize.x, texelSize.y)).rgb;
      kernel[7] = SAMPLE_MAIN(uv + float2(0.0, texelSize.y)).rgb;
      kernel[8] = SAMPLE_MAIN(uv + float2(texelSize.x, texelSize.y)).rgb;

      // Convert to luminance for edge detection
      float lum[9];
      for (int i = 0; i < 9; i++)
        lum[i] = dot(kernel[i], float3(0.299, 0.587, 0.114));

      // Sobel operator on luminance
      float sobelX = lum[0] * -1.0 + lum[2] * 1.0 +
                     lum[3] * -2.0 + lum[5] * 2.0 +
                     lum[6] * -1.0 + lum[8] * 1.0;

      float sobelY = lum[0] * -1.0 + lum[1] * -2.0 + lum[2] * -1.0 +
                     lum[6] * 1.0 + lum[7] * 2.0 + lum[8] * 1.0;

      lineWeight = sqrt(sobelX * sobelX + sobelY * sobelY);

#elif defined(_EDGES_HYBRID)
      // Hybrid method combining depth and color detection

      // Depth component
      float4 pix = float4(TEXEL_SIZE.xy, -TEXEL_SIZE.xy) * _EdgeWidth;
      float4 depthEven = float4(SampleLinear01Depth(uv + float2(0.0, pix.w)),
                                SampleLinear01Depth(uv + float2(0.0, pix.y)),
                                SampleLinear01Depth(uv + float2(pix.x, 0.0)),
                                SampleLinear01Depth(uv + float2(pix.z, 0.0)));
      float depthEdge = max4(abs(depthEven - depthClamped));

      // Color component
      float2 texelSize = TEXEL_SIZE.xy * _EdgeWidth;
      float3 centerColor = SAMPLE_MAIN(uv).rgb;
      float3 sampleColor = SAMPLE_MAIN(uv + texelSize).rgb;
      float colorEdge = distance(centerColor, sampleColor);

      // Combine depth and color edges
      lineWeight = max(depthEdge, colorEdge);

#else
      float4 pix = float4(TEXEL_SIZE.xy, -TEXEL_SIZE.xy) * _EdgeWidth;
      float4 depthEven = float4(SampleLinear01Depth(uv + float2(0.0, pix.w)),
                                SampleLinear01Depth(uv + float2(0.0, pix.y)),
                                SampleLinear01Depth(uv + float2(pix.x, 0.0)),
                                SampleLinear01Depth(uv + float2(pix.z, 0.0)));
      float2 retVal = abs(depthEven.xz - depthEven.yw);
      lineWeight = max(retVal.x, retVal.y);
#endif

      // Apply border mask to suppress false edges at screen borders
      lineWeight *= borderMask;

      // Hard threshold for crisp edges
      lineWeight = step(_EdgeThreshold, lineWeight);

      // Determine background color
      float3 backgroundColor = ColorBlend(_BackgroundBlend, color.rgb, _BackgroundColor);

      // Calculate edge color (with optional depth-based coloring)
      float3 edgeColor = ColorBlend(_EdgesBlend, color.rgb, _EdgesColor);
#if defined(_EDGES_DEPTH_COLOR)
      float pixelDepth = SampleLinear01Depth(uv);
      float depthFactor = saturate((pixelDepth - _DepthEdgeNear) / max(_DepthEdgeFar - _DepthEdgeNear, 0.0001));
      float3 depthEdgeColor = lerp(_DepthEdgeNearColor, _DepthEdgeFarColor, depthFactor);
      edgeColor = ColorBlend(_EdgesBlend, color.rgb, depthEdgeColor);
#endif

      // Apply edge detection
      pixel = lerp(backgroundColor, edgeColor, lineWeight * _Edges);

      // Apply color adjustments first
      pixel = ColorAdjust(pixel, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

#if 0
      pixel = PixelDemo(color.rgb, pixel, uv);
#endif

      // Final blend with intensity
      return float4(lerp(color.rgb, pixel, _Intensity), color.a);
    }
    ENDHLSL

    Pass
    {
      Name "Fronkon Games Weird Edges"

      HLSLPROGRAM
      #pragma vertex WeirdVert
      #pragma fragment EdgesFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _EDGES_DEPTH_COLOR
      #pragma multi_compile _ _EDGES_IGNORE_SKYBOX
      #pragma multi_compile _EDGES_DEPTH_NEIGHBORS _EDGES_SOBEL _EDGES_PREWITT _EDGES_NORMAL_BASED _EDGES_COLOR_BASED _EDGES_HYBRID
      ENDHLSL
    }
  }

  FallBack "Diffuse"
}
