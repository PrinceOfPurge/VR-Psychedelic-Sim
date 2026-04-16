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

namespace FronkonGames.Weird.Bubbles
{
  /// <summary> Bubbles Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Bubbles"), HelpURL(Constants.Support.Documentation)]
  public sealed class BubblesVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Bubbles settings.

    /// <summary> Bubble size [10, 100]. Default 40. </summary>
    [FloatSliderWithReset(40.0f, 10.0f, 100.0f, "Bubble size [10, 100]. Default 40.")]
    public FloatSliderParameterNoInterpolation bubbleSize = new(40.0f, 10.0f, 100.0f);

    /// <summary> Bubble color. Default white. </summary>
    [ColorWithReset(0xFFFFFFFF, "Bubble color. Default white.")]
    public ColorParameterNoInterpolation bubbleColor = new(Color.white);

    /// <summary> Bubble color blend mode with the original color. Default Solid. </summary>
    [EnumDropdown(0, "Bubble color blend mode with the original color. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> bubbleColorBlend = new(ColorBlends.Solid);

    /// <summary> Bevel [0.05, 1]. Default 0.4. </summary>
    [FloatSliderWithReset(0.4f, 0.05f, 1.0f, "Bevel [0.05, 1]. Default 0.4.")]
    public FloatSliderParameterNoInterpolation bubbleBevel = new(0.4f, 0.05f, 1.0f);

    /// <summary> Spacing between bubbles. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Spacing between bubbles. Default 0.1.")]
    public FloatSliderParameterNoInterpolation bubbleSpacing = new(0.1f, 0.0f, 1.0f);

    /// <summary> Bubble shape roundness [0, 1]. Default 1. </summary>
    /// <remarks> 0 = square, 1 = circle. </remarks>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Bubble shape roundness [0, 1]. Default 1. 0 = square, 1 = circle.")]
    public FloatSliderParameterNoInterpolation bubbleRoundness = new(1.0f, 0.0f, 1.0f);

    /// <summary> Background color. Default white. </summary>
    [ColorWithReset(0xFFFFFFFF, "Background color. Default white.")]
    public ColorParameterNoInterpolation backgroundColor = new(Color.white);

    /// <summary> Background blend mode with the original color. Default Solid. </summary>
    [EnumDropdown(0, "Background blend mode with the original color. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> backgroundBlend = new(ColorBlends.Solid);

    /// <summary> Background blur radius in pixels [0, 20]. Default 8. </summary>
    [FloatSliderWithReset(8.0f, 0.0f, 20.0f, "Background blur radius in pixels [0, 20]. Default 8.")]
    public FloatSliderParameterNoInterpolation backgroundBlur = new(8.0f, 0.0f, 20.0f);

    /// <summary> Background exposure value [-2.0, 2.0]. Default -0.6. </summary>
    [FloatSliderWithReset(-0.6f, -2.0f, 2.0f, "Background exposure value [-2.0, 2.0]. Default -0.6.")]
    public FloatSliderParameterNoInterpolation backgroundExposure = new(-0.6f, -2.0f, 2.0f);

    /// <summary> Specular intensity. Default 0.45. </summary>
    [FloatSliderWithReset(0.45f, 0.0f, 2.0f, "Specular intensity. Default 0.45.")]
    public FloatSliderParameterNoInterpolation lightSpecular = new(0.45f, 0.0f, 2.0f);

    /// <summary> Light color. Default white. </summary>
    [ColorWithReset(0xFFFFFFFF, "Light color. Default white.")]
    public ColorParameterNoInterpolation lightColor = new(Color.white);

    /// <summary> Specular power. Default 70. </summary>
    [FloatSliderWithReset(70.0f, 0.0f, 100.0f, "Specular power. Default 70.")]
    public FloatSliderParameterNoInterpolation lightSpecularPower = new(70.0f, 0.0f, 100.0f);

    /// <summary> Light angle/orientation [0, 360]. Default 45. </summary>
    [FloatSliderWithReset(45.0f, 0.0f, 360.0f, "Light angle/orientation [0, 360]. Default 45.")]
    public FloatSliderParameterNoInterpolation lightAngle = new(45.0f, 0.0f, 360.0f);

    /// <summary> Light elevation [0, 90]. Default 46. </summary>
    /// <remarks> 0 = horizontal, 90 = vertical (from above). </remarks>
    [FloatSliderWithReset(46.0f, 0.0f, 90.0f, "Light elevation [0, 90]. Default 46. 0 = horizontal, 90 = vertical (from above).")]
    public FloatSliderParameterNoInterpolation lightElevation = new(46.0f, 0.0f, 90.0f);

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

    public static Color DefaultShadowTint = new(0.0f, 0.5f, 0.9f);
    public static Vector3 DefaultFlareRGB = new(0.35f, 0.28f, 0.21f);

    /// <summary> Reset to default values. </summary>
    public void Reset()
    {
      intensity.value = 1.0f;


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