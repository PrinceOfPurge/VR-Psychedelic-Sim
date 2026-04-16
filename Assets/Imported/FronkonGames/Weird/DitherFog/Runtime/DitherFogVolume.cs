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
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FronkonGames.Weird.DitherFog
{
  /// <summary> Dither Fog Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Dither Fog"), HelpURL(Constants.Support.Documentation)]
  public sealed class DitherFogVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Dither Fog settings.

    /// <summary> Fog opacity [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Fog opacity [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation fogOpacity = new(1.0f, 0.0f, 1.0f);

    /// <summary> Fog color mode. Default Solid. </summary>
    [EnumDropdown(0, "Fog color mode. Default Solid.")]
    public EnumParameterNoInterpolation<FogColorModes> fogColorMode = new(FogColorModes.Solid);

    /// <summary> Fog color (used in Solid mode). Default black. </summary>
    [ColorWithReset(0xFF000000, "Fog color (used in Solid mode). Default black.")]
    public ColorParameterNoInterpolation fogColor = new(Color.black);

    /// <summary> Fog gradient near color (used in Gradient mode). Default transparent black. </summary>
    [ColorWithReset(0x00000000, "Fog gradient near color (used in Gradient mode). Default transparent.")]
    public ColorParameterNoInterpolation fogGradientNear = new(new Color(0.0f, 0.0f, 0.0f, 0.0f));

    /// <summary> Fog gradient far color (used in Gradient mode). Default black. </summary>
    [ColorWithReset(0xFF000000, "Fog gradient far color (used in Gradient mode). Default black.")]
    public ColorParameterNoInterpolation fogGradientFar = new(new Color(0.0f, 0.0f, 0.0f, 1.0f));

    /// <summary> Dithering mode. Default Bayer8x8. </summary>
    [EnumDropdown(1, "Dithering mode. Default Bayer8x8.")]
    public EnumParameterNoInterpolation<DitheringModes> ditheringMode = new(DitheringModes.Bayer8x8);

    /// <summary> Dither pattern scale [1, 8]. Larger values create bigger pixels. Default 1. </summary>
    [IntSliderWithReset(1, 1, 8, "Dither pattern scale [1, 8]. Larger values create bigger pixels. Default 1.")]
    public ClampedIntParameterNoInterpolation ditherScale = new(1, 1, 8);

    /// <summary> Enable adaptive dithering that adjusts based on content. Default false. </summary>
    [ToggleWithReset(false, "Enable adaptive dithering that adjusts based on content. Default false.")]
    public BoolParameterNoInterpolation adaptiveDithering = new(false);

    /// <summary> Adaptive brightness threshold [0, 1]. Adjust dithering strength based on scene brightness. Default 0.25. </summary>
    [FloatSliderWithReset(0.25f, 0.0f, 1.0f, "Adaptive brightness threshold [0, 1]. Default 0.25.")]
    public FloatSliderParameterNoInterpolation adaptiveBrightnessThreshold = new(0.25f, 0.0f, 1.0f);

    /// <summary> Adaptive contrast sensitivity [0, 2]. How much contrast affects dithering. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 2.0f, "Adaptive contrast sensitivity [0, 2]. Default 1.")]
    public FloatSliderParameterNoInterpolation adaptiveContrastSensitivity = new(1.0f, 0.0f, 2.0f);

    /// <summary> Color quantization level (0 = disabled, 8 = 8-bit, 16 = 16-bit, etc.) [0, 255]. Default 0. </summary>
    [IntSliderWithReset(0, 0, 255, "Color quantization level [0, 255]. Default 0 (disabled).")]
    public ClampedIntParameterNoInterpolation quantize = new(0, 0, 255);

    /// <summary> Fog curve start (near fog intensity) [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Fog curve start [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation fogCurveStart = new(0.1f, 0.0f, 1.0f);

    /// <summary> Fog curve end (far fog intensity) [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Fog curve end [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation fogCurveEnd = new(1.0f, 0.0f, 1.0f);

    /// <summary> Distance at which the fog center is away from the camera [0, 1]. Default 0.25. </summary>
    [FloatSliderWithReset(0.25f, 0.0f, 1.0f, "Fog start distance [0, 1]. Default 0.25.")]
    public FloatSliderParameterNoInterpolation fogStart = new(0.25f, 0.0f, 1.0f);

    /// <summary> If enabled the fog will curve around the start position. Default true. </summary>
    [ToggleWithReset(true, "Enable curved fog falloff. Default true.")]
    public BoolParameterNoInterpolation curvedFog = new(true);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Color settings.

    /// <summary> Brightness [-1, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -1.0f, 1.0f, "Brightness [-1, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation brightness = new(0.0f, -1.0f, 1.0f);

    /// <summary> Contrast [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Contrast [0, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation contrast = new(1.0f, 0.0f, 10.0f);

    /// <summary> Gamma [0.1, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Gamma [0.1, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation gamma = new(1.0f, 0.1f, 10.0f);

    /// <summary> The color wheel [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "The color wheel [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation hue = new(0.0f, 0.0f, 1.0f);

    /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 2.0f, "Intensity of a colors [0, 2]. Default 1.")]
    public FloatSliderParameterNoInterpolation saturation = new(1.0f, 0.0f, 2.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Advanced settings.

    /// <summary> Does it affect the Scene View? </summary>
    [ToggleWithReset(false, "Does it affect the Scene View?")]
    public BoolParameterNoInterpolation affectSceneView = new(false);

    /// <summary> Use scaled time. </summary>
    [ToggleWithReset(true, "Use scaled time.")]
    public BoolParameterNoInterpolation useScaledTime = new(true);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary> Reset to default values. </summary>
    public void Reset()
    {
      intensity.value = 1.0f;

      fogOpacity.value = 1.0f;
      fogColorMode.value = FogColorModes.Solid;
      fogColor.value = Color.black;
      fogGradientNear.value = new Color(0.0f, 0.0f, 0.0f, 0.0f);
      fogGradientFar.value = new Color(0.0f, 0.0f, 0.0f, 1.0f);
      ditheringMode.value = DitheringModes.Bayer8x8;
      ditherScale.value = 1;
      adaptiveDithering.value = false;
      adaptiveBrightnessThreshold.value = 0.25f;
      adaptiveContrastSensitivity.value = 1.0f;
      quantize.value = 0;
      fogCurveStart.value = 0.1f;
      fogCurveEnd.value = 1.0f;
      fogStart.value = 0.25f;
      curvedFog.value = true;

      brightness.value = 0.0f;
      contrast.value = 1.0f;
      gamma.value = 1.0f;
      hue.value = 0.0f;
      saturation.value = 1.0f;

      affectSceneView.value = false;
      useScaledTime.value = true;
    }

    /// <summary> Is the effect active? </summary>
    public bool IsActive() => intensity.overrideState && intensity.value > 0.0f;

    /// <summary> Is the effect tile compatible? </summary>
    public bool IsTileCompatible() => false;
  }
}
