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

namespace FronkonGames.Weird.Crystal
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Crystal
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private CrystalVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int CrystalIntensity = Shader.PropertyToID("_CrystalIntensity");
        internal static readonly int CrystalColorBlend = Shader.PropertyToID("_CrystalColorBlend");
        internal static readonly int CrystalGain = Shader.PropertyToID("_CrystalGain");
        internal static readonly int CrystalSpeed = Shader.PropertyToID("_CrystalSpeed");
        internal static readonly int CrystalColor = Shader.PropertyToID("_CrystalColor");
        internal static readonly int CrystalScale = Shader.PropertyToID("_CrystalScale");
        internal static readonly int CrystalPower = Shader.PropertyToID("_CrystalPower");
        internal static readonly int CrystalRotation0 = Shader.PropertyToID("_CrystalRotation0");
        internal static readonly int CrystalRotation1 = Shader.PropertyToID("_CrystalRotation1");
        internal static readonly int CrystalReflection = Shader.PropertyToID("_CrystalReflection");
        internal static readonly int CrystalRefraction = Shader.PropertyToID("_CrystalRefraction");

        internal static readonly int LightsIntensity = Shader.PropertyToID("_LightsIntensity");
        internal static readonly int LightsColorBlend = Shader.PropertyToID("_LightsColorBlend");
        internal static readonly int LightsSpeed = Shader.PropertyToID("_LightsSpeed");
        internal static readonly int LightsIterations = Shader.PropertyToID("_LightsIterations");
        internal static readonly int LightsColorOffset = Shader.PropertyToID("_LightsColorOffset");
        internal static readonly int LightsComplexity = Shader.PropertyToID("_LightsComplexity");
        internal static readonly int LightsDistortion = Shader.PropertyToID("_LightsDistortion");
        internal static readonly int LightsSpread = Shader.PropertyToID("_LightsSpread");
        internal static readonly int LightsRotationSpeed = Shader.PropertyToID("_LightsRotationSpeed");
        internal static readonly int LightsTurbulence = Shader.PropertyToID("_LightsTurbulence");
        internal static readonly int LightsDetail = Shader.PropertyToID("_LightsDetail");
        internal static readonly int LightsWarp = Shader.PropertyToID("_LightsWarp");
        internal static readonly int LightsBrightness = Shader.PropertyToID("_LightsBrightness");
        internal static readonly int LightsContrast = Shader.PropertyToID("_LightsContrast");
        internal static readonly int LightsPower = Shader.PropertyToID("_LightsPower");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static readonly string UseCrystal = "USE_CRYSTAL";
        internal static readonly string UseLights = "USE_LIGHTS";
        internal static readonly string UseRefraction = "USE_REFRACTION";
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

        if (volume.crystalIntensity.value > 0.0f)
        {
          material.EnableKeyword(Keywords.UseCrystal);
          material.SetFloat(ShaderIDs.CrystalIntensity, volume.crystalIntensity.value);
          material.SetInt(ShaderIDs.CrystalColorBlend, (int)volume.crystalColorBlend.value);
          material.SetVector(ShaderIDs.CrystalColor, volume.crystalColor.value);
          material.SetFloat(ShaderIDs.CrystalGain, volume.crystalGain.value);
          material.SetFloat(ShaderIDs.CrystalScale, volume.crystalScale.value);
          material.SetFloat(ShaderIDs.CrystalSpeed, volume.crystalSpeed.value);
          material.SetFloat(ShaderIDs.CrystalPower, volume.crystalPower.value);
          material.SetFloat(ShaderIDs.CrystalRotation0, volume.crystalRotation0.value);
          material.SetFloat(ShaderIDs.CrystalRotation1, volume.crystalRotation1.value);

          if (volume.crystalRefraction.value > 0.0f)
          {
            material.EnableKeyword(Keywords.UseRefraction);
            material.SetFloat(ShaderIDs.CrystalReflection, volume.crystalReflection.value);
            material.SetFloat(ShaderIDs.CrystalRefraction, volume.crystalRefraction.value);
          }
        }

        if (volume.lightsIntensity.value > 0.0f)
        {
          material.EnableKeyword(Keywords.UseLights);
          material.SetFloat(ShaderIDs.LightsIntensity, volume.lightsIntensity.value);
          material.SetInt(ShaderIDs.LightsColorBlend, (int)volume.lightsColorBlend.value);
          material.SetFloat(ShaderIDs.LightsSpeed, volume.lightsSpeed.value);
          material.SetInt(ShaderIDs.LightsIterations, volume.lightsIterations.value);
          material.SetVector(ShaderIDs.LightsColorOffset, volume.lightsColorOffset.value);
          material.SetFloat(ShaderIDs.LightsComplexity, volume.lightsComplexity.value);
          material.SetFloat(ShaderIDs.LightsDistortion, volume.lightsDistortion.value);
          material.SetFloat(ShaderIDs.LightsSpread, volume.lightsSpread.value);
          material.SetFloat(ShaderIDs.LightsRotationSpeed, volume.lightsRotationSpeed.value);
          material.SetFloat(ShaderIDs.LightsTurbulence, volume.lightsTurbulence.value);
          material.SetFloat(ShaderIDs.LightsDetail, volume.lightsDetail.value);
          material.SetFloat(ShaderIDs.LightsWarp, volume.lightsWarp.value);
          material.SetFloat(ShaderIDs.LightsBrightness, volume.lightsBrightness.value);
          material.SetFloat(ShaderIDs.LightsContrast, volume.lightsContrast.value);
          material.SetFloat(ShaderIDs.LightsPower, volume.lightsPower.value);
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
        volume = VolumeManager.instance.stack.GetComponent<CrystalVolume>();
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
