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

namespace FronkonGames.Weird.DitherFog
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class DitherFog
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private DitherFogVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int FogOpacity = Shader.PropertyToID("_FogOpacity");
        internal static readonly int FogColorMode = Shader.PropertyToID("_FogColorMode");
        internal static readonly int FogColor = Shader.PropertyToID("_FogColor");
        internal static readonly int FogGradientNear = Shader.PropertyToID("_FogGradientNear");
        internal static readonly int FogGradientFar = Shader.PropertyToID("_FogGradientFar");
        internal static readonly int DitheringMode = Shader.PropertyToID("_DitheringMode");
        internal static readonly int DitherScale = Shader.PropertyToID("_DitherScale");
        internal static readonly int AdaptiveDithering = Shader.PropertyToID("_AdaptiveDithering");
        internal static readonly int AdaptiveBrightnessThreshold = Shader.PropertyToID("_AdaptiveBrightnessThreshold");
        internal static readonly int AdaptiveContrastSensitivity = Shader.PropertyToID("_AdaptiveContrastSensitivity");
        internal static readonly int Quantize = Shader.PropertyToID("_Quantize");
        internal static readonly int FogCurve = Shader.PropertyToID("_FogCurve");
        internal static readonly int FogStart = Shader.PropertyToID("_FogStart");
        internal static readonly int CurvedFog = Shader.PropertyToID("_CurvedFog");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
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

        material.SetFloat(ShaderIDs.FogOpacity, volume.fogOpacity.value);
        material.SetInt(ShaderIDs.FogColorMode, (int)volume.fogColorMode.value);
        material.SetColor(ShaderIDs.FogColor, volume.fogColor.value);
        material.SetColor(ShaderIDs.FogGradientNear, volume.fogGradientNear.value);
        material.SetColor(ShaderIDs.FogGradientFar, volume.fogGradientFar.value);
        material.SetInt(ShaderIDs.DitheringMode, (int)volume.ditheringMode.value);
        material.SetInt(ShaderIDs.DitherScale, volume.ditherScale.value * Mathf.Max(1, Screen.height / 440));
        material.SetInt(ShaderIDs.AdaptiveDithering, volume.adaptiveDithering.value ? 1 : 0);
        material.SetFloat(ShaderIDs.AdaptiveBrightnessThreshold, volume.adaptiveBrightnessThreshold.value);
        material.SetFloat(ShaderIDs.AdaptiveContrastSensitivity, volume.adaptiveContrastSensitivity.value);
        material.SetInt(ShaderIDs.Quantize, volume.quantize.value);
        material.SetVector(ShaderIDs.FogCurve, new Vector2(volume.fogCurveStart.value, volume.fogCurveEnd.value));
        material.SetFloat(ShaderIDs.FogStart, volume.fogStart.value);
        material.SetInt(ShaderIDs.CurvedFog, volume.curvedFog.value ? 1 : 0);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<DitherFogVolume>();
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
