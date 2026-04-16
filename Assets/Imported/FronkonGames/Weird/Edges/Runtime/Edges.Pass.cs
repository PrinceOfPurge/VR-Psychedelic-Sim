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
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace FronkonGames.Weird.Edges
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Edges
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private EdgesVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        // Edges settings
        internal static readonly int Edges = Shader.PropertyToID("_Edges");
        internal static readonly int EdgeWidth = Shader.PropertyToID("_EdgeWidth");
        internal static readonly int EdgeThreshold = Shader.PropertyToID("_EdgeThreshold");
        internal static readonly int EdgeDetectionMethod = Shader.PropertyToID("_EdgeDetectionMethod");
        internal static readonly int EdgesColor = Shader.PropertyToID("_EdgesColor");
        internal static readonly int EdgesBlend = Shader.PropertyToID("_EdgesBlend");
        internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
        internal static readonly int BackgroundBlend = Shader.PropertyToID("_BackgroundBlend");
        internal static readonly int DepthEdgeNearColor = Shader.PropertyToID("_DepthEdgeNearColor");
        internal static readonly int DepthEdgeFarColor = Shader.PropertyToID("_DepthEdgeFarColor");
        internal static readonly int DepthEdgeNear = Shader.PropertyToID("_DepthEdgeNear");
        internal static readonly int DepthEdgeFar = Shader.PropertyToID("_DepthEdgeFar");

        // Color settings
        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static readonly string Noise = "_EDGES_NOISE";
        internal static readonly string Glow = "_EDGES_GLOW";
        internal static readonly string DepthColor = "_EDGES_DEPTH_COLOR";
        internal static readonly string IgnoreSkybox = "_EDGES_IGNORE_SKYBOX";

        internal static readonly string DepthNeighbors = "_EDGES_DEPTH_NEIGHBORS";
        internal static readonly string Sobel =          "_EDGES_SOBEL";
        internal static readonly string Prewitt =        "_EDGES_PREWITT";
        internal static readonly string NormalBased =    "_EDGES_NORMAL_BASED";
        internal static readonly string ColorBased =     "_EDGES_COLOR_BASED";
        internal static readonly string Hybrid =         "_EDGES_HYBRID";
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass() : base()
      {
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
      }

      /// <summary> Destroy the render pass. </summary>
      ~RenderPass() => material = null;

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;
        switch (volume.edgeDetectionMethod.value)
        {
          case EdgeDetectionMethod.DepthNeighbors:  material.EnableKeyword(Keywords.DepthNeighbors); break;
          case EdgeDetectionMethod.Sobel:           material.EnableKeyword(Keywords.Sobel); break;
          case EdgeDetectionMethod.Prewitt:         material.EnableKeyword(Keywords.Prewitt); break;
          case EdgeDetectionMethod.NormalBased:     material.EnableKeyword(Keywords.NormalBased); break;
          case EdgeDetectionMethod.ColorBased:      material.EnableKeyword(Keywords.ColorBased); break;
          case EdgeDetectionMethod.Hybrid:          material.EnableKeyword(Keywords.Hybrid); break;
        }

        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        // Edges settings
        material.SetFloat(ShaderIDs.Edges, volume.edges.value);
        material.SetInt(ShaderIDs.EdgeWidth, volume.edgeWidth.value);
        material.SetFloat(ShaderIDs.EdgeThreshold, volume.edgeThreshold.value);

        material.SetFloat(ShaderIDs.EdgeDetectionMethod, (float)volume.edgeDetectionMethod.value);
        material.SetInt(ShaderIDs.EdgesBlend, (int)volume.edgesBlend.value);
        material.SetInt(ShaderIDs.BackgroundBlend, (int)volume.backgroundBlend.value);
        material.SetColor(ShaderIDs.BackgroundColor, volume.backgroundColor.value);

        if (volume.edgeColorMode.value == EdgeColorMode.DepthBased)
        {
          material.EnableKeyword(Keywords.DepthColor);
          material.SetColor(ShaderIDs.DepthEdgeNearColor, volume.depthEdgeNearColor.value);
          material.SetColor(ShaderIDs.DepthEdgeFarColor, volume.depthEdgeFarColor.value);
          material.SetFloat(ShaderIDs.DepthEdgeNear, volume.depthEdgeNear.value);
          material.SetFloat(ShaderIDs.DepthEdgeFar, volume.depthEdgeFar.value);
        }
        else
        {
          material.DisableKeyword(Keywords.DepthColor);
          material.SetColor(ShaderIDs.EdgesColor, volume.edgesColor.value);
        }

        if (volume.ignoreSkybox.value == true)
          material.EnableKeyword(Keywords.IgnoreSkybox);

        // Color settings
        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<EdgesVolume>();
        if (material == null || volume == null || volume.IsActive() == false)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == true)
          return;

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.camera.cameraType == CameraType.SceneView && volume.affectSceneView.value == false || cameraData.postProcessEnabled == false)
          return;

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = renderGraph.CreateTexture(source.GetDescriptor(renderGraph));

        UpdateMaterial();

        RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);
        renderGraph.AddBlitPass(pass, $"{Constants.Asset.AssemblyName}.Pass");

        resourceData.cameraColor = destination;
      }
    }
  }
}
