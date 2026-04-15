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

namespace FronkonGames.Weird.Kaleidoscope
{
  /// <summary> Kaleidoscope Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Kaleidoscope"), HelpURL(Constants.Support.Documentation)]
  public sealed class KaleidoscopeVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Kaleidoscope settings.

    /// <summary> Center of the effect [0, 1]. Default (0.5, 0.5). </summary>
    [Vector2WithReset(0.5f, 0.5f, "Center of the effect [0, 1]. Default (0.5, 0.5).")]
    public Vector2ParameterNoInterpolation center = new(new Vector2(0.5f, 0.5f));

    /// <summary> Iteration count [1, 10]. Default 6. </summary>
    [IntSliderWithReset(6, 1, 10, "Iteration count [1, 10]. Default 6.")]
    public ClampedIntParameterNoInterpolation iterationCount = new(6, 1, 10);

    /// <summary> Strength curve. Default Linear from edge to center. </summary>
    public AnimationCurveParameterNoInterpolation strength = new(DefaultStrength);

    /// <summary> Speed of the animation [-10, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, -10.0f, 10.0f, "Speed of the animation [-10, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation speed = new(1.0f, -10.0f, 10.0f);

    /// <summary> Scale of the effect [0.1, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Scale of the effect [0.1, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation scale = new(1.0f, 0.1f, 10.0f);

    /// <summary> Keep aspect ratio. Default false. </summary>
    [ToggleWithReset(false, "Keep aspect ratio. Default false.")]
    public BoolParameterNoInterpolation keepAspectRatio = new(false);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Offset UV settings.

    /// <summary> Offset UV intensity [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Offset UV intensity [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation offsetIntensity = new(0.0f, 0.0f, 1.0f);

    /// <summary> Offset red scale. Default (10.0, 10.0). </summary>
    [Vector2WithReset(10.0f, 10.0f, "Offset red scale. Default (10.0, 10.0).")]
    public Vector2ParameterNoInterpolation offsetRedScale = new(new Vector2(10.0f, 10.0f));

    /// <summary> Offset green scale. Default (-10.0, 10.0). </summary>
    [Vector2WithReset(-10.0f, 10.0f, "Offset green scale. Default (-10.0, 10.0).")]
    public Vector2ParameterNoInterpolation offsetGreenScale = new(new Vector2(-10.0f, 10.0f));

    /// <summary> Offset blue scale. Default (10.0, -10.0). </summary>
    [Vector2WithReset(10.0f, -10.0f, "Offset blue scale. Default (10.0, -10.0).")]
    public Vector2ParameterNoInterpolation offsetBlueScale = new(new Vector2(10.0f, -10.0f));

    /// <summary> Offset UV scale [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Offset UV scale [0, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation offsetScale = new(1.0f, 0.0f, 10.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Color settings.

    /// <summary> Color intensity [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Color intensity [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation colorIntensity = new(1.0f, 0.0f, 1.0f);

    /// <summary> Color palette preset. Default Original. </summary>
    [EnumDropdown(0, "Color palette preset. Default Original.")]
    public EnumParameterNoInterpolation<ColorPalettes> colorPalette = new(ColorPalettes.Original);

    /// <summary> Blend mode. Default Additive. </summary>
    [EnumDropdown(0, "Blend mode. Default Additive.")]
    public EnumParameterNoInterpolation<ColorBlends> blend = new(ColorBlends.Additive);

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
    #region Segment settings.

    /// <summary> Segment intensity [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Segment intensity [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation segmentIntensity = new(1.0f, 0.0f, 1.0f);

    /// <summary> Segment color. Default Black. </summary>
    [ColorWithReset(0x000000FF, "Segment color. Default Black.")]
    public ColorParameterNoInterpolation segmentColor = new(Color.black);

    /// <summary> Segment blend mode. Default Solid. </summary>
    [EnumDropdown(16, "Segment blend mode. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> segmentBlend = new(ColorBlends.Solid);

    /// <summary> Segment width [0, 1]. Default 0.25. </summary>
    [FloatSliderWithReset(0.25f, 0.0f, 1.0f, "Segment width [0, 1]. Default 0.25.")]
    public FloatSliderParameterNoInterpolation segmentWidth = new(0.25f, 0.0f, 1.0f);

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

    public static AnimationCurve DefaultStrength => new(new Keyframe(0.0f, 1.0f),
                                                        new Keyframe(0.8f, 0.0f),
                                                        new Keyframe(1.0f, 0.0f));

    /// <summary> Reset to default values. </summary>
    public void Reset()
    {
      intensity.value = 1.0f;

      center.value = new Vector2(0.5f, 0.5f);
      iterationCount.value = 6;
      strength.value = new AnimationCurve(DefaultStrength.keys);
      speed.value = 1.0f;
      scale.value = 1.0f;
      keepAspectRatio.value = false;

      offsetIntensity.value = 0.0f;
      offsetRedScale.value = new Vector2(10.0f, 10.0f);
      offsetGreenScale.value = new Vector2(-10.0f, 10.0f);
      offsetBlueScale.value = new Vector2(10.0f, -10.0f);
      offsetScale.value = 1.0f;

      colorIntensity.value = 1.0f;
      colorPalette.value = ColorPalettes.Original;
      blend.value = ColorBlends.Additive;

      brightness.value = 0.0f;
      contrast.value = 1.0f;
      gamma.value = 1.0f;
      hue.value = 0.0f;
      saturation.value = 1.0f;

      segmentIntensity.value = 1.0f;
      segmentColor.value = Color.black;
      segmentBlend.value = ColorBlends.Solid;
      segmentWidth.value = 0.25f;

      affectSceneView.value = false;
      useScaledTime.value = true;
    }

    /// <summary> Is the effect active? </summary>
    public bool IsActive() => intensity.overrideState && intensity.value > 0.0f;

    /// <summary> Is the effect tile compatible? </summary>
    public bool IsTileCompatible() => false;
  }
}
