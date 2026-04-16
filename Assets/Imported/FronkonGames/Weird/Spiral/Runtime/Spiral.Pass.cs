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

namespace FronkonGames.Weird.Spiral
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Spiral
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private SpiralVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int ShapeType = Shader.PropertyToID("_ShapeType");
        internal static readonly int SpiralAmount = Shader.PropertyToID("_SpiralAmount");
        internal static readonly int Rotation = Shader.PropertyToID("_Rotation");
        internal static readonly int RotationSpeed = Shader.PropertyToID("_RotationSpeed");
        internal static readonly int OuterRing = Shader.PropertyToID("_OuterRing");
        internal static readonly int ZoomSpeed = Shader.PropertyToID("_ZoomSpeed");
        internal static readonly int Frequency = Shader.PropertyToID("_Frequency");
        internal static readonly int Center = Shader.PropertyToID("_Center");
        internal static readonly int Wrap = Shader.PropertyToID("_Wrap");
        internal static readonly int EdgeMode = Shader.PropertyToID("_EdgeMode");

        internal static readonly int OuterTintIntensity = Shader.PropertyToID("_OuterTintIntensity");
        internal static readonly int OuterTintColorBlend = Shader.PropertyToID("_OuterTintColorBlend");
        internal static readonly int OuterTintColor = Shader.PropertyToID("_OuterTintColor");
        internal static readonly int OuterTintSoftness = Shader.PropertyToID("_OuterTintSoftness");

        internal static readonly int ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
        internal static readonly int ShadowColorBlend = Shader.PropertyToID("_ShadowColorBlend");
        internal static readonly int ShadowColor = Shader.PropertyToID("_ShadowColor");
        internal static readonly int ShadowSoftness = Shader.PropertyToID("_ShadowSoftness");
        internal static readonly int ShadowOffset = Shader.PropertyToID("_ShadowOffset");

        internal static readonly int LineWidth = Shader.PropertyToID("_LineWidth");
        internal static readonly int LineColor = Shader.PropertyToID("_LineColor");
        internal static readonly int LineColorBlend = Shader.PropertyToID("_LineColorBlend");
        internal static readonly int LineSoftness = Shader.PropertyToID("_LineSoftness");
        internal static readonly int LineCount = Shader.PropertyToID("_LineCount");

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
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        material.SetInt(ShaderIDs.ShapeType, (int)volume.shape.value);
        material.SetVector(ShaderIDs.Center, volume.center.value);
        material.SetFloat(ShaderIDs.SpiralAmount, volume.spiralAmount.value);
        material.SetFloat(ShaderIDs.Rotation, volume.rotation.value * Mathf.Deg2Rad);
        material.SetFloat(ShaderIDs.RotationSpeed, volume.rotationSpeed.value);
        material.SetFloat(ShaderIDs.OuterRing, volume.outerRing.value);
        material.SetFloat(ShaderIDs.ZoomSpeed, volume.zoomSpeed.value);
        material.SetFloat(ShaderIDs.Frequency, volume.frequency.value);
        material.SetFloat(ShaderIDs.Wrap, volume.wrap.value);
        material.SetInt(ShaderIDs.EdgeMode, (int)volume.edgeMode.value);

        material.SetFloat(ShaderIDs.OuterTintIntensity, volume.outerTintIntensity.value);
        material.SetInt(ShaderIDs.OuterTintColorBlend, (int)volume.outerTintColorBlend.value);
        material.SetColor(ShaderIDs.OuterTintColor, volume.outerTintColor.value);
        material.SetFloat(ShaderIDs.OuterTintSoftness, volume.outerTintSoftness.value * 10.0f);

        material.SetFloat(ShaderIDs.ShadowIntensity, volume.shadowIntensity.value);
        material.SetInt(ShaderIDs.ShadowColorBlend, (int)volume.shadowColorBlend.value);
        material.SetColor(ShaderIDs.ShadowColor, volume.shadowColor.value);
        material.SetFloat(ShaderIDs.ShadowSoftness, volume.shadowSoftness.value);
        material.SetFloat(ShaderIDs.ShadowOffset, volume.shadowOffset.value);

        material.SetFloat(ShaderIDs.LineWidth, volume.lineWidth.value);
        material.SetColor(ShaderIDs.LineColor, volume.lineColor.value);
        material.SetInt(ShaderIDs.LineColorBlend, (int)volume.lineColorBlend.value);
        material.SetFloat(ShaderIDs.LineSoftness, volume.lineSoftness.value);
        material.SetInt(ShaderIDs.LineCount, volume.lineCount.value);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<SpiralVolume>();
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
