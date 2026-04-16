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

namespace FronkonGames.Weird.Crystal
{
  /// <summary> Crystal Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Crystal"), HelpURL(Constants.Support.Documentation)]
  public sealed class CrystalVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Crystal settings.

    /// <summary> Crystal intensity [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Crystal intensity [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation crystalIntensity = new(1.0f, 0.0f, 1.0f);

    /// <summary> Crystal color blend operation. Default Additive. </summary>
    [EnumDropdown(0, "Crystal color blend operation. Default Additive.")]
    public EnumParameterNoInterpolation<ColorBlends> crystalColorBlend = new(ColorBlends.Additive);

    /// <summary> Crystal color. Default (0.3, 0.8, 1.2). </summary>
    [Vector3WithReset(0.3f, 0.8f, 1.2f, "Crystal color. Default (0.3, 0.8, 1.2).")]
    public Vector3ParameterNoInterpolation crystalColor = new(new Vector3(0.3f, 0.8f, 1.2f));

    /// <summary> Crystal gain [0, 2]. Default 0.45. </summary>
    [FloatSliderWithReset(0.45f, 0.0f, 2.0f, "Crystal gain [0, 2]. Default 0.45.")]
    public FloatSliderParameterNoInterpolation crystalGain = new(0.45f, 0.0f, 2.0f);

    /// <summary> Crystal scale [0.1, 10]. Default 2.2. </summary>
    [FloatSliderWithReset(2.2f, 0.1f, 10.0f, "Crystal scale [0.1, 10]. Default 2.2.")]
    public FloatSliderParameterNoInterpolation crystalScale = new(2.2f, 0.1f, 10.0f);

    /// <summary> Crystal animation speed [0, 2]. Default 0.5. </summary>
    [FloatSliderWithReset(0.5f, 0.0f, 2.0f, "Crystal animation speed [0, 2]. Default 0.5.")]
    public FloatSliderParameterNoInterpolation crystalSpeed = new(0.5f, 0.0f, 2.0f);

    /// <summary> Crystal power [0.1, 20]. Default 5.0. </summary>
    [FloatSliderWithReset(5.0f, 0.1f, 20.0f, "Crystal power [0.1, 20]. Default 5.0.")]
    public FloatSliderParameterNoInterpolation crystalPower = new(5.0f, 0.1f, 20.0f);

    /// <summary> Crystal rotation #0 [0, 180]. Default 30.0. </summary>
    [FloatSliderWithReset(30.0f, 0.0f, 180.0f, "Crystal rotation #0 [0, 180]. Default 30.0.")]
    public FloatSliderParameterNoInterpolation crystalRotation0 = new(30.0f, 0.0f, 180.0f);

    /// <summary> Crystal rotation #1 [0, 180]. Default 5.0. </summary>
    [FloatSliderWithReset(5.0f, 0.0f, 180.0f, "Crystal rotation #1 [0, 180]. Default 5.0.")]
    public FloatSliderParameterNoInterpolation crystalRotation1 = new(5.0f, 0.0f, 180.0f);

    /// <summary> Crystal reflection amount [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Crystal reflection amount [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation crystalReflection = new(0.1f, 0.0f, 1.0f);

    /// <summary> Crystal refraction strength (scale and deformation) [0, 10]. Default 1.0. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Crystal refraction strength (scale and deformation) [0, 10]. Default 1.0.")]
    public FloatSliderParameterNoInterpolation crystalRefraction = new(1.0f, 0.0f, 10.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Lights settings.

    /// <summary> Lights intensity [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Lights intensity [0, 1]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation lightsIntensity = new(0.1f, 0.0f, 1.0f);

    /// <summary> Lights color blend operation. Default Screen. </summary>
    [EnumDropdown(15, "Lights color blend operation. Default Screen.")]
    public EnumParameterNoInterpolation<ColorBlends> lightsColorBlend = new(ColorBlends.Screen);

    /// <summary> Lights animation speed [0, 2]. Default 0.5. </summary>
    [FloatSliderWithReset(0.5f, 0.0f, 2.0f, "Lights animation speed [0, 2]. Default 0.5.")]
    public FloatSliderParameterNoInterpolation lightsSpeed = new(0.5f, 0.0f, 2.0f);

    /// <summary> Lights iterations (detail/complexity) [5, 30]. Default 19. </summary>
    [IntSliderWithReset(19, 5, 30, "Lights iterations (detail/complexity) [5, 30]. Default 19.")]
    public ClampedIntParameterNoInterpolation lightsIterations = new(19, 5, 30);

    /// <summary> Lights color offset (RGB shift). Default (1, 2, 3). </summary>
    [Vector3WithReset(1.0f, 2.0f, 3.0f, "Lights color offset (RGB shift). Default (1, 2, 3).")]
    public Vector3ParameterNoInterpolation lightsColorOffset = new(new Vector3(1.0f, 2.0f, 3.0f));

    /// <summary> Lights complexity growth rate [0.01, 0.1]. Default 0.03. </summary>
    [FloatSliderWithReset(0.03f, 0.01f, 0.1f, "Lights complexity growth rate [0.01, 0.1]. Default 0.03.")]
    public FloatSliderParameterNoInterpolation lightsComplexity = new(0.03f, 0.01f, 0.1f);

    /// <summary> Lights spatial distortion [1, 15]. Default 7.0. </summary>
    [FloatSliderWithReset(7.0f, 1.0f, 15.0f, "Lights spatial distortion [1, 15]. Default 7.0.")]
    public FloatSliderParameterNoInterpolation lightsDistortion = new(7.0f, 1.0f, 15.0f);

    /// <summary> Lights pattern spread [1, 10]. Default 5.0. </summary>
    [FloatSliderWithReset(5.0f, 1.0f, 10.0f, "Lights pattern spread [1, 10]. Default 5.0.")]
    public FloatSliderParameterNoInterpolation lightsSpread = new(5.0f, 1.0f, 10.0f);

    /// <summary> Lights rotation animation speed [0, 0.1]. Default 0.02. </summary>
    [FloatSliderWithReset(0.02f, 0.0f, 0.1f, "Lights rotation animation speed [0, 0.1]. Default 0.02.")]
    public FloatSliderParameterNoInterpolation lightsRotationSpeed = new(0.02f, 0.0f, 0.1f);

    /// <summary> Lights turbulence intensity [10, 100]. Default 40.0. </summary>
    [FloatSliderWithReset(40.0f, 10.0f, 100.0f, "Lights turbulence intensity [10, 100]. Default 40.0.")]
    public FloatSliderParameterNoInterpolation lightsTurbulence = new(40.0f, 10.0f, 100.0f);

    /// <summary> Lights detail level [0.5, 3.0]. Default 1.5. </summary>
    [FloatSliderWithReset(1.5f, 0.5f, 3.0f, "Lights detail level [0.5, 3.0]. Default 1.5.")]
    public FloatSliderParameterNoInterpolation lightsDetail = new(1.5f, 0.5f, 3.0f);

    /// <summary> Lights spatial warp amount [3, 15]. Default 9.0. </summary>
    [FloatSliderWithReset(9.0f, 3.0f, 15.0f, "Lights spatial warp amount [3, 15]. Default 9.0.")]
    public FloatSliderParameterNoInterpolation lightsWarp = new(9.0f, 3.0f, 15.0f);

    /// <summary> Lights brightness multiplier [5, 50]. Default 25.6. </summary>
    [FloatSliderWithReset(25.6f, 5.0f, 50.0f, "Lights brightness multiplier [5, 50]. Default 25.6.")]
    public FloatSliderParameterNoInterpolation lightsBrightness = new(25.6f, 5.0f, 50.0f);

    /// <summary> Lights contrast level [5, 30]. Default 13.0. </summary>
    [FloatSliderWithReset(13.0f, 5.0f, 30.0f, "Lights contrast level [5, 30]. Default 13.0.")]
    public FloatSliderParameterNoInterpolation lightsContrast = new(13.0f, 5.0f, 30.0f);

    /// <summary> Lights final power curve [1, 10]. Default 5.0. </summary>
    [FloatSliderWithReset(5.0f, 1.0f, 10.0f, "Lights final power curve [1, 10]. Default 5.0.")]
    public FloatSliderParameterNoInterpolation lightsPower = new(5.0f, 1.0f, 10.0f);

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

    public static readonly Vector3 DefaultCrystalColor = new(0.3f, 0.8f, 1.2f);
    public static readonly Vector3 DefaultLightsColorOffset = new(1.0f, 2.0f, 3.0f);

    /// <summary> Reset to default values. </summary>
    public void Reset()
    {
      intensity.value = 1.0f;

      crystalIntensity.value = 1.0f;
      crystalColorBlend.value = ColorBlends.Additive;
      crystalColor.value = DefaultCrystalColor;
      crystalGain.value = 0.45f;
      crystalScale.value = 2.2f;
      crystalSpeed.value = 0.5f;
      crystalPower.value = 5.0f;
      crystalRotation0.value = 30.0f;
      crystalRotation1.value = 5.0f;
      crystalReflection.value = 0.1f;
      crystalRefraction.value = 1.0f;

      lightsIntensity.value = 0.1f;
      lightsColorBlend.value = ColorBlends.Screen;
      lightsSpeed.value = 0.5f;
      lightsIterations.value = 19;
      lightsColorOffset.value = DefaultLightsColorOffset;
      lightsComplexity.value = 0.03f;
      lightsDistortion.value = 7.0f;
      lightsSpread.value = 5.0f;
      lightsRotationSpeed.value = 0.02f;
      lightsTurbulence.value = 40.0f;
      lightsDetail.value = 1.5f;
      lightsWarp.value = 9.0f;
      lightsBrightness.value = 25.6f;
      lightsContrast.value = 13.0f;
      lightsPower.value = 5.0f;

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
