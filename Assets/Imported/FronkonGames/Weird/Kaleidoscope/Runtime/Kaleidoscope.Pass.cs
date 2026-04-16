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

namespace FronkonGames.Weird.Kaleidoscope
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Kaleidoscope
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private KaleidoscopeVolume volume;

      private Texture2D strengthCurveTexture;
      private const int StrengthCurveResolution = 256;
      private int cachedKeyframeCount = -1;
      private float cachedCurveHash = -1.0f;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int EffectTime = Shader.PropertyToID("_EffectTime");

        internal static readonly int Center = Shader.PropertyToID("_Center");
        internal static readonly int IterationCount = Shader.PropertyToID("_IterationCount");
        internal static readonly int StrengthCurve = Shader.PropertyToID("_StrengthCurve");
        internal static readonly int Speed = Shader.PropertyToID("_Speed");
        internal static readonly int Scale = Shader.PropertyToID("_Scale");
        internal static readonly int KeepAspectRatio = Shader.PropertyToID("_KeepAspectRatio");

        internal static readonly int OffsetIntensity = Shader.PropertyToID("_OffsetIntensity");
        internal static readonly int OffsetRedScale = Shader.PropertyToID("_OffsetRedScale");
        internal static readonly int OffsetGreenScale = Shader.PropertyToID("_OffsetGreenScale");
        internal static readonly int OffsetBlueScale = Shader.PropertyToID("_OffsetBlueScale");
        internal static readonly int OffsetScale = Shader.PropertyToID("_OffsetScale");

        internal static readonly int ColorIntensity = Shader.PropertyToID("_ColorIntensity");
        internal static readonly int ColorPalette = Shader.PropertyToID("_ColorPalette");
        internal static readonly int Blend = Shader.PropertyToID("_Blend");
        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");

        internal static readonly int SegmentIntensity = Shader.PropertyToID("_SegmentIntensity");
        internal static readonly int SegmentColor = Shader.PropertyToID("_SegmentColor");
        internal static readonly int SegmentBlend = Shader.PropertyToID("_SegmentBlend");
        internal static readonly int SegmentWidth = Shader.PropertyToID("_SegmentWidth");
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass() : base()
      {
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
      }

      /// <summary> Destroy the render pass. </summary>
      ~RenderPass()
      {
        material = null;
        if (strengthCurveTexture != null)
        {
          Object.DestroyImmediate(strengthCurveTexture);
          strengthCurveTexture = null;
        }
      }

      private float ComputeCurveHash(AnimationCurve curve)
      {
        if (curve == null || curve.length == 0)
          return 0.0f;

        float hash = curve.length;
        for (int i = 0; i < curve.length; ++i)
        {
          Keyframe key = curve[i];
          hash = hash * 31.0f + key.time;
          hash = hash * 31.0f + key.value;
          hash = hash * 31.0f + key.inTangent;
          hash = hash * 31.0f + key.outTangent;
        }

        return hash;
      }

      private bool HasCurveChanged()
      {
        if (volume.strength.value == null)
          return false;

        int currentKeyframeCount = volume.strength.value.length;
        if (currentKeyframeCount != cachedKeyframeCount)
          return true;

        float currentHash = ComputeCurveHash(volume.strength.value);
        if (Mathf.Abs(currentHash - cachedCurveHash) > 0.0001f)
          return true;

        return false;
      }

      private void UpdateStrengthCurveTexture()
      {
        if (strengthCurveTexture == null)
        {
          strengthCurveTexture = new Texture2D(StrengthCurveResolution, 1, TextureFormat.RFloat, false)
          {
            name = "KaleidoscopeStrengthCurve",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
          };
        }

        Color[] pixels = new Color[StrengthCurveResolution];
        for (int i = 0; i < StrengthCurveResolution; ++i)
        {
          float t = i / (float)(StrengthCurveResolution - 1);
          float value = volume.strength.value.Evaluate(t);
          pixels[i] = new Color(value, 0.0f, 0.0f, 0.0f);
        }

        strengthCurveTexture.SetPixels(pixels);
        strengthCurveTexture.Apply(false, false);

        // Update cache
        cachedKeyframeCount = volume.strength.value.length;
        cachedCurveHash = ComputeCurveHash(volume.strength.value);
      }

      private void UpdateMaterial()
      {
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        float time = volume.useScaledTime.value == true ? Time.time : Time.unscaledTime;
        material.SetVector(ShaderIDs.EffectTime, new Vector4(time / 20.0f, time, time * 2.0f, time * 3.0f));

        if (strengthCurveTexture == null || HasCurveChanged() == true)
          UpdateStrengthCurveTexture();

        material.SetVector(ShaderIDs.Center, volume.center.value);
        material.SetInt(ShaderIDs.IterationCount, volume.iterationCount.value);
        material.SetTexture(ShaderIDs.StrengthCurve, strengthCurveTexture);
        material.SetFloat(ShaderIDs.Speed, volume.speed.value);
        material.SetFloat(ShaderIDs.Scale, volume.scale.value);
        material.SetFloat(ShaderIDs.KeepAspectRatio, volume.keepAspectRatio.value ? 1.0f : 0.0f);

        material.SetFloat(ShaderIDs.OffsetIntensity, volume.offsetIntensity.value);
        material.SetVector(ShaderIDs.OffsetRedScale, volume.offsetRedScale.value);
        material.SetVector(ShaderIDs.OffsetGreenScale, volume.offsetGreenScale.value);
        material.SetVector(ShaderIDs.OffsetBlueScale, volume.offsetBlueScale.value);
        material.SetFloat(ShaderIDs.OffsetScale, volume.offsetScale.value * Screen.width * 0.5f);

        material.SetFloat(ShaderIDs.ColorIntensity, volume.colorIntensity.value);
        material.SetInt(ShaderIDs.ColorPalette, (int)volume.colorPalette.value);
        material.SetInt(ShaderIDs.Blend, (int)volume.blend.value);
        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);

        material.SetFloat(ShaderIDs.SegmentIntensity, volume.segmentIntensity.value);
        material.SetColor(ShaderIDs.SegmentColor, volume.segmentColor.value);
        material.SetInt(ShaderIDs.SegmentBlend, (int)volume.segmentBlend.value);
        material.SetFloat(ShaderIDs.SegmentWidth, 1.0f - volume.segmentWidth.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<KaleidoscopeVolume>();
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
