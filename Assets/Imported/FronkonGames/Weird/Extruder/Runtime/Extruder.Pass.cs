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

namespace FronkonGames.Weird.Extruder
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Extruder
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private ExtruderVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int GridScale = Shader.PropertyToID("_GridScale");
        internal static readonly int DepthScale = Shader.PropertyToID("_DepthScale");
        internal static readonly int DepthRemapMin = Shader.PropertyToID("_DepthRemapMin");
        internal static readonly int DepthRemapMax = Shader.PropertyToID("_DepthRemapMax");
        internal static readonly int LuminosityRemapMin = Shader.PropertyToID("_LuminosityRemapMin");
        internal static readonly int LuminosityRemapMax = Shader.PropertyToID("_LuminosityRemapMax");
        internal static readonly int MaxRayDistance = Shader.PropertyToID("_MaxRayDistance");
        internal static readonly int StepMultiplier = Shader.PropertyToID("_StepMultiplier");
        internal static readonly int ShadowSoftness = Shader.PropertyToID("_ShadowSoftness");
        internal static readonly int RaymarchingSteps = Shader.PropertyToID("_RaymarchingSteps");
        internal static readonly int CameraDistance = Shader.PropertyToID("_CameraDistance");
        internal static readonly int LightPosition = Shader.PropertyToID("_LightPosition");
        internal static readonly int LightColor = Shader.PropertyToID("_LightColor");
        internal static readonly int SpecularColor = Shader.PropertyToID("_SpecularColor");
        internal static readonly int ShadowIterations = Shader.PropertyToID("_ShadowIterations");
        internal static readonly int FresnelIntensity = Shader.PropertyToID("_FresnelIntensity");
        internal static readonly int AmbientOcclusionIterations = Shader.PropertyToID("_AmbientOcclusionIterations");
        internal static readonly int Rotation = Shader.PropertyToID("_Rotation");
        internal static readonly int ColorBlend = Shader.PropertyToID("_ColorBlend");
        internal static readonly int FloorColor = Shader.PropertyToID("_FloorColor");
        internal static readonly int FloorColorBlend = Shader.PropertyToID("_FloorColorBlend");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static readonly string Shadows = "SHADOWS";
        internal static readonly string AmbientOcclusion = "AMBIENT_OCCLUSION";
        internal static readonly string Grayscale = "GRAYSCALE";
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
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        material.SetFloat(ShaderIDs.GridScale, volume.gridScale.value * 0.01f);
        material.SetFloat(ShaderIDs.DepthScale, volume.depthScale.value);
        material.SetFloat(ShaderIDs.MaxRayDistance, volume.maxRayDistance.value);
        material.SetFloat(ShaderIDs.StepMultiplier, volume.stepMultiplier.value);
        material.SetFloat(ShaderIDs.ShadowSoftness, volume.shadowSoftness.value);
        material.SetInt(ShaderIDs.RaymarchingSteps, volume.raymarchingSteps.value);
        material.SetFloat(ShaderIDs.CameraDistance, volume.cameraDistance.value);
        material.SetVector(ShaderIDs.Rotation, volume.rotation.value * Mathf.Deg2Rad);
        material.SetVector(ShaderIDs.LightPosition, volume.lightPosition.value);
        material.SetColor(ShaderIDs.LightColor, volume.lightColor.value);
        material.SetColor(ShaderIDs.SpecularColor, volume.specularColor.value);
        material.SetFloat(ShaderIDs.FresnelIntensity, volume.fresnelIntensity.value);
        material.SetInt(ShaderIDs.ColorBlend, (int)volume.colorBlend.value);
        material.SetColor(ShaderIDs.FloorColor, volume.floorColor.value);
        material.SetInt(ShaderIDs.FloorColorBlend, (int)volume.floorColorBlend.value);

        if (volume.heightMethod.value == HeightMethod.Grayscale)
        {
          material.EnableKeyword(Keywords.Grayscale);
          material.SetFloat(ShaderIDs.LuminosityRemapMin, volume.luminosityRemapMin.value);
          material.SetFloat(ShaderIDs.LuminosityRemapMax, volume.luminosityRemapMax.value);
        }
        else
        {
          material.SetFloat(ShaderIDs.DepthRemapMin, volume.depthRemapMin.value);
          material.SetFloat(ShaderIDs.DepthRemapMax, volume.depthRemapMax.value);
        }

        if (volume.shadowIterations.value > 0)
        {
          material.EnableKeyword(Keywords.Shadows);
          material.SetInt(ShaderIDs.ShadowIterations, volume.shadowIterations.value);
        }

        if (volume.ambientOcclusionIterations.value > 0)
        {
          material.EnableKeyword(Keywords.AmbientOcclusion);
          material.SetInt(ShaderIDs.AmbientOcclusionIterations, volume.ambientOcclusionIterations.value);
        }

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<ExtruderVolume>();
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
