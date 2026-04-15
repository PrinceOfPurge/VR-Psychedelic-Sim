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

namespace FronkonGames.Weird.Pinch
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Pinch
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private PinchVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int Start = Shader.PropertyToID("_Start");
        internal static readonly int End = Shader.PropertyToID("_End");
        internal static readonly int StartRadius = Shader.PropertyToID("_StartRadius");
        internal static readonly int EndRadius = Shader.PropertyToID("_EndRadius");
        internal static readonly int RaymarchSteps = Shader.PropertyToID("_RaymarchSteps");
        internal static readonly int MaxDistance = Shader.PropertyToID("_MaxDistance");
        internal static readonly int Roundness = Shader.PropertyToID("_Roundness");
        internal static readonly int LightDirection = Shader.PropertyToID("_LightDirection");
        internal static readonly int LightIntensity = Shader.PropertyToID("_LightIntensity");

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

      private float ExpoEaseOut(float t) => Mathf.Pow(2.0f, 10.0f * (t - 1.0f));

      private void UpdateMaterial()
      {
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        material.SetVector(ShaderIDs.Start, volume.start.value);
        material.SetVector(ShaderIDs.End, volume.end.value + volume.start.value);
        material.SetFloat(ShaderIDs.StartRadius, volume.startRadius.value * 0.1f);
        material.SetFloat(ShaderIDs.EndRadius, volume.endRadius.value * 0.25f);
        material.SetFloat(ShaderIDs.RaymarchSteps, volume.raymarchSteps.value);
        material.SetFloat(ShaderIDs.MaxDistance, volume.maxDistance.value);
        material.SetFloat(ShaderIDs.Roundness, volume.roundness.value);
        material.SetFloat(ShaderIDs.LightIntensity, Mathf.Lerp(0.0f, volume.lightIntensity.value, ExpoEaseOut(Vector2.Distance(volume.start.value, volume.end.value + volume.start.value) * 5.0f)));
        material.SetVector(ShaderIDs.LightDirection, volume.lightDirection.value);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<PinchVolume>();
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
