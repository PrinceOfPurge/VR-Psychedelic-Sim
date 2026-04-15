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

namespace FronkonGames.Weird.Spiral
{
  /// <summary> Spiral Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Spiral"), HelpURL(Constants.Support.Documentation)]
  public sealed class SpiralVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Spiral settings.

    /// <summary> Smooth wrap/unwrap transition between original image and droste effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Smooth wrap/unwrap transition between original image and droste effect [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation wrap = new(1.0f, 0.0f, 1.0f);

    /// <summary> Shape of the recursive appearance. Default Circular. </summary>
    [EnumDropdown(0, "Shape of the recursive appearance. Default Circular.")]
    public EnumParameterNoInterpolation<ShapeType> shape = new(ShapeType.Circular);

    /// <summary> Center of the effect [-0.5, 0.5]. Default (0.0, 0.0). </summary>
    [Vector2WithReset(0.0f, 0.0f, "Center of the effect [-0.5, 0.5]. Default (0.0, 0.0).")]
    public Vector2ParameterNoInterpolation center = new(Vector2.zero);

    /// <summary> Spiral twist amount [0, 1]. 0 = no spiral, 1 = full spiral. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Spiral twist amount [0, 1]. 0 = no spiral, 1 = full spiral. Default 1.")]
    public FloatSliderParameterNoInterpolation spiralAmount = new(1.0f, 0.0f, 1.0f);

    /// <summary> Static rotation angle [0, 360]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 360.0f, "Static rotation angle [0, 360]. Default 0.")]
    public FloatSliderParameterNoInterpolation rotation = new(0.0f, 0.0f, 360.0f);

    /// <summary> Rotation animation speed [-2, 2]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -2.0f, 2.0f, "Rotation animation speed [-2, 2]. Default 0.")]
    public FloatSliderParameterNoInterpolation rotationSpeed = new(0.0f, -2.0f, 2.0f);

    /// <summary> The outer ring defines the texture border towards the edge of the screen [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "The outer ring defines the texture border towards the edge of the screen [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation outerRing = new(1.0f, 0.0f, 1.0f);

    /// <summary> Zoom animation speed [-2, 2]. Negative = zoom out, Positive = zoom in. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -2.0f, 2.0f, "Zoom animation speed [-2, 2]. Negative = zoom out, Positive = zoom in. Default 0.")]
    public FloatSliderParameterNoInterpolation zoomSpeed = new(0.0f, -2.0f, 2.0f);

    /// <summary> Defines the frequency of the recursion [0.1, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 5.0f, "Defines the frequency of the recursion [0.1, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation frequency = new(1.0f, 0.1f, 5.0f);

    /// <summary> How to handle UVs outside [0,1] bounds. Default Mirror. </summary>
    [EnumDropdown(1, "How to handle UVs outside [0,1] bounds. Default Mirror.")]
    public EnumParameterNoInterpolation<EdgeMode> edgeMode = new(EdgeMode.Mirror);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Outer Tint settings.

    /// <summary> Outer tint intensity [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Outer tint intensity [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation outerTintIntensity = new(0.0f, 0.0f, 1.0f);

    /// <summary> Outer tint color blend mode. Default Solid. </summary>
    [EnumDropdown(17, "Outer tint color blend mode. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> outerTintColorBlend = new(ColorBlends.Solid);

    /// <summary> Outer tint color. Default cyan. </summary>
    [ColorWithReset(0x00FFFFFF, "Outer tint color. Default cyan.")]
    public ColorParameterNoInterpolation outerTintColor = new(Color.cyan);

    /// <summary> Outer tint softness/falloff [0.1, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 5.0f, "Outer tint softness/falloff [0.1, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation outerTintSoftness = new(1.0f, 0.1f, 5.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Shadow settings.

    /// <summary> Shadow intensity based on recursion depth [0, 1]. Default 0.2. </summary>
    [FloatSliderWithReset(0.2f, 0.0f, 1.0f, "Shadow intensity based on recursion depth [0, 1]. Default 0.2.")]
    public FloatSliderParameterNoInterpolation shadowIntensity = new(0.2f, 0.0f, 1.0f);

    /// <summary> Shadow color blend mode. Default Multiply. </summary>
    [EnumDropdown(13, "Shadow color blend mode. Default Multiply.")]
    public EnumParameterNoInterpolation<ColorBlends> shadowColorBlend = new(ColorBlends.Multiply);

    /// <summary> Shadow color. Default black. </summary>
    [ColorWithReset(0x000000FF, "Shadow color. Default black.")]
    public ColorParameterNoInterpolation shadowColor = new(Color.black);

    /// <summary> Shadow softness/falloff [0.1, 5]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 5.0f, "Shadow softness/falloff [0.1, 5]. Default 1.")]
    public FloatSliderParameterNoInterpolation shadowSoftness = new(1.0f, 0.1f, 5.0f);

    /// <summary> Shadow offset towards inner recursions [-1, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -1.0f, 1.0f, "Shadow offset towards inner recursions [-1, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation shadowOffset = new(0.0f, -1.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Line settings.

    /// <summary> Line width at recursion boundaries [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Line width at recursion boundaries [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation lineWidth = new(0.0f, 0.0f, 1.0f);

    /// <summary> Line color. Default white. </summary>
    [ColorWithReset(0xFFFFFFFF, "Line color. Default white.")]
    public ColorParameterNoInterpolation lineColor = new(Color.white);

    /// <summary> Line color blend mode. Default Solid. </summary>
    [EnumDropdown(17, "Line color blend mode. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> lineColorBlend = new(ColorBlends.Solid);

    /// <summary> Line edge softness [0.01, 1]. Default 0.5. </summary>
    [FloatSliderWithReset(0.5f, 0.01f, 1.0f, "Line edge softness [0.01, 1]. Default 0.5.")]
    public FloatSliderParameterNoInterpolation lineSoftness = new(0.5f, 0.01f, 1.0f);

    /// <summary> Number of lines per recursion [1, 10]. Default 1. </summary>
    [IntSliderWithReset(1, 1, 10, "Number of lines per recursion [1, 10]. Default 1.")]
    public ClampedIntParameterNoInterpolation lineCount = new(1, 1, 10);

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

      wrap.value = 1.0f;
      shape.value = ShapeType.Circular;
      center.value = Vector2.zero;
      spiralAmount.value = 1.0f;
      rotation.value = 0.0f;
      rotationSpeed.value = 0.0f;
      outerRing.value = 1.0f;
      zoomSpeed.value = 0.0f;
      frequency.value = 1.0f;
      edgeMode.value = EdgeMode.Mirror;

      outerTintIntensity.value = 0.0f;
      outerTintColorBlend.value = ColorBlends.Solid;
      outerTintColor.value = Color.cyan;
      outerTintSoftness.value = 1.0f;

      shadowIntensity.value = 0.2f;
      shadowColorBlend.value = ColorBlends.Multiply;
      shadowColor.value = Color.black;
      shadowSoftness.value = 1.0f;
      shadowOffset.value = 0.0f;

      lineWidth.value = 0.0f;
      lineColor.value = Color.white;
      lineColorBlend.value = ColorBlends.Solid;
      lineSoftness.value = 0.5f;
      lineCount.value = 1;

      brightness.value = 0.0f;
      contrast.value = 1.0f;
      gamma.value = 1.0f;
      hue.value = 0.0f;
      saturation.value = 1.0f;

      affectSceneView.value = false;
      useScaledTime.value = true;
    }

    /// <summary> Is the effect active? </summary>
    public bool IsActive() => intensity.overrideState && intensity.value > 0.0f && wrap.value > 0.0f;

    /// <summary> Is the effect tile compatible? </summary>
    public bool IsTileCompatible() => false;
  }
}
