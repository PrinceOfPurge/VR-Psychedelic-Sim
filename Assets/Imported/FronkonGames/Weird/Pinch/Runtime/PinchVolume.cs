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

namespace FronkonGames.Weird.Pinch
{
  /// <summary> Pinch Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Pinch"), HelpURL(Constants.Support.Documentation)]
  public sealed class PinchVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Pinch settings.

    /// <summary> Center of the effect. Default (0.5, 0.5). </summary>
    [Vector2WithReset(0.5f, 0.5f, "Center of the effect. Default (0.5, 0.5).")]
    public Vector2ParameterNoInterpolation start = new(new Vector2(0.5f, 0.5f));

    /// <summary> Point/pointer position for distortion. Default (0, 0). </summary>
    [Vector2WithReset(0.0f, 0.0f, "Point/pointer position for distortion. Default (0, 0).")]
    public Vector2ParameterNoInterpolation end = new(Vector2.zero);

    /// <summary> Start area radius [0, 1]. Controls the size of the pinch effect at the start position. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Start area radius [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation startRadius = new(0.0f, 0.0f, 1.0f);

    /// <summary> End area radius [0, 1]. Controls the size of the pinch effect at the end position. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "End area radius [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation endRadius = new(0.1f, 0.0f, 1.0f);

    /// <summary> Roundness of the pinch effect [0, 1]. Higher values create softer, more rounded pinches. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Roundness of the pinch effect [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation roundness = new(0.1f, 0.0f, 1.0f);

    /// <summary> Light direction for Lambert lighting. Default (0.577, 0.577, 0.577). </summary>
    [Vector3WithReset(0.577f, 0.577f, 0.577f, "Light direction for Lambert lighting. Default (0.577, 0.577, 0.577).")]
    public Vector3ParameterNoInterpolation lightDirection = new(new Vector3(0.577f, 0.577f, 0.577f));

    /// <summary> Light intensity [0, 1]. Controls how much Lambert lighting affects the result. Default 0.5. </summary>
    [FloatSliderWithReset(0.5f, 0.0f, 1.0f, "Light intensity [0, 1]. Default 0.5.")]
    public FloatSliderParameterNoInterpolation lightIntensity = new(0.5f, 0.0f, 1.0f);

    /// <summary> Number of raymarching steps [1, 100]. Default 100. </summary>
    [FloatSliderWithReset(100.0f, 1.0f, 100.0f, "Number of raymarching steps [1, 100]. Default 100.")]
    public FloatSliderParameterNoInterpolation raymarchSteps = new(100.0f, 1.0f, 100.0f);

    /// <summary> Maximum raymarching distance [0.1, 10]. Default 1.0. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Maximum raymarching distance [0.1, 10]. Default 1.0.")]
    public FloatSliderParameterNoInterpolation maxDistance = new(1.0f, 0.1f, 10.0f);

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

      start.value = new Vector2(0.5f, 0.5f);
      end.value = Vector2.zero;
      startRadius.value = 0.0f;
      endRadius.value = 0.1f;
      roundness.value = 0.1f;
      lightDirection.value = new Vector3(0.577f, 0.577f, 0.577f);
      lightIntensity.value = 0.5f;
      raymarchSteps.value = 100.0f;
      maxDistance.value = 1.0f;

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
