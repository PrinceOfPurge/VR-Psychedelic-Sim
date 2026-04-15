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

namespace FronkonGames.Weird.Bubbles
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Bubbles
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private BubblesVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int Cell = Shader.PropertyToID("_Cell");
        internal static readonly int BevelK = Shader.PropertyToID("_BevelK");
        internal static readonly int BlurPx = Shader.PropertyToID("_BlurPx");
        internal static readonly int EV = Shader.PropertyToID("_EV");
        internal static readonly int Spacing = Shader.PropertyToID("_Spacing");
        internal static readonly int BubbleRoundness = Shader.PropertyToID("_BubbleRoundness");
        internal static readonly int SpecPwr = Shader.PropertyToID("_SpecPwr");
        internal static readonly int SpecInt = Shader.PropertyToID("_SpecInt");
        internal static readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
        internal static readonly int BackgroundBlend = Shader.PropertyToID("_BackgroundBlend");
        internal static readonly int BubbleColor = Shader.PropertyToID("_BubbleColor");
        internal static readonly int BubbleColorBlend = Shader.PropertyToID("_BubbleColorBlend");
        internal static readonly int LightColor = Shader.PropertyToID("_LightColor");
        internal static readonly int LightAngle = Shader.PropertyToID("_LightAngle");
        internal static readonly int LightElevation = Shader.PropertyToID("_LightElevation");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static readonly string UseBlur = "USE_BLUR";
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
        
        material.SetFloat(ShaderIDs.Cell, volume.bubbleSize.value);
        material.SetColor(ShaderIDs.BubbleColor, volume.bubbleColor.value);
        material.SetInt(ShaderIDs.BubbleColorBlend, (int)volume.bubbleColorBlend.value);
        material.SetFloat(ShaderIDs.BevelK, volume.bubbleBevel.value);
        material.SetFloat(ShaderIDs.Spacing, volume.bubbleSpacing.value * 10.0f);
        material.SetFloat(ShaderIDs.BubbleRoundness, volume.bubbleRoundness.value);

        material.SetColor(ShaderIDs.BackgroundColor, volume.backgroundColor.value);
        material.SetFloat(ShaderIDs.EV, volume.backgroundExposure.value);
        material.SetInt(ShaderIDs.BackgroundBlend, (int)volume.backgroundBlend.value);
        if (volume.backgroundBlur.value > 0.0f)
        {
          material.EnableKeyword(Keywords.UseBlur);
          material.SetFloat(ShaderIDs.BlurPx, volume.backgroundBlur.value);
        }

        material.SetFloat(ShaderIDs.SpecPwr, volume.lightSpecularPower.value);
        material.SetFloat(ShaderIDs.SpecInt, volume.lightSpecular.value);
        material.SetColor(ShaderIDs.LightColor, volume.lightColor.value);
        material.SetFloat(ShaderIDs.LightAngle, volume.lightAngle.value);
        material.SetFloat(ShaderIDs.LightElevation, volume.lightElevation.value);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<BubblesVolume>();
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
