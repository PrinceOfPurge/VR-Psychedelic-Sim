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

namespace FronkonGames.Weird.FireTunnel
{
  /// <summary> Fire Tunnel Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Fire Tunnel"), HelpURL(Constants.Support.Documentation)]
  public sealed class FireTunnelVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Fire Tunnel settings.

    /// <summary> Center of the effect. Default (0, 0). </summary>
    [Vector2WithReset(0.0f, 0.0f, "Center of the effect. Default (0, 0).")]
    public Vector2ParameterNoInterpolation center = new(Vector2.zero);

    /// <summary> Animation speed multiplier [-5, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, -5.0f, 5.0f, "Animation speed multiplier [-5, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation speed = new(1.0f, -5.0f, 5.0f);

    /// <summary> Tunnel radius scale [0, 10]. Default 4. </summary>
    [FloatSliderWithReset(4.0f, 0.0f, 10.0f, "Tunnel radius scale [0, 10]. Default 4.")]
    public FloatSliderParameterNoInterpolation tunnelRadius = new(4.0f, 0.0f, 10.0f);

    /// <summary> Turbulence strength [0, 2]. Default 0.6. </summary>
    [FloatSliderWithReset(0.6f, 0.0f, 2.0f, "Turbulence strength [0, 2]. Default 0.6.")]
    public FloatSliderParameterNoInterpolation turbulence = new(0.6f, 0.0f, 2.0f);

    /// <summary> Rotation speed multiplier [-5, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, -5.0f, 5.0f, "Rotation speed multiplier [-5, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation rotation = new(1.0f, -5.0f, 5.0f);

    /// <summary> Number of raymarching steps [10, 200]. Default 100. </summary>
    [IntSliderWithReset(100, 10, 200, "Number of raymarching steps [10, 200]. Default 100.")]
    public ClampedIntParameterNoInterpolation raymarchSteps = new(100, 10, 200);

    /// <summary> Noise detail/scale [0.5, 5]. Default 1.12. </summary>
    [FloatSliderWithReset(1.12f, 0.5f, 5.0f, "Noise detail/scale [0.5, 5]. Default 1.12.")]
    public FloatSliderParameterNoInterpolation noiseScale = new(1.12f, 0.5f, 5.0f);

    /// <summary> Fire intensity multiplier [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Fire intensity multiplier [0, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation fireIntensity = new(1.0f, 0.0f, 10.0f);

    /// <summary> Color blend operation. Default Additive. </summary>
    [EnumDropdown((int)ColorBlends.Additive, "Color blend operation. Default Additive.")]
    public EnumParameterNoInterpolation<ColorBlends> colorBlend = new(ColorBlends.Additive);

    /// <summary> Fire color. Default (5, 2, 1). </summary>
    [Vector3WithReset(5.0f, 2.0f, 1.0f, "Fire color. Default (5, 2, 1).")]
    public Vector3ParameterNoInterpolation fireColor = new(new Vector3(5.0f, 2.0f, 1.0f));

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

      center.value = Vector2.zero;
      speed.value = 1.0f;
      tunnelRadius.value = 4.0f;
      turbulence.value = 0.6f;
      rotation.value = 1.0f;
      raymarchSteps.value = 100;
      noiseScale.value = 1.12f;
      fireIntensity.value = 1.0f;
      colorBlend.value = ColorBlends.Additive;
      fireColor.value = new Vector3(5.0f, 2.0f, 1.0f);

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
