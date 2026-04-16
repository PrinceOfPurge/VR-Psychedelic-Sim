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

namespace FronkonGames.Weird.Extruder
{
  /// <summary> Extruder Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Extruder"), HelpURL(Constants.Support.Documentation)]
  public sealed class ExtruderVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Extruder settings.

    /// <summary> Grid scale (smaller = larger blocks) [1, 20]. Default 3. </summary>
    [FloatSliderWithReset(3.0f, 1.0f, 20.0f, "Grid scale (smaller = larger blocks) [1, 20]. Default 3.")]
    public FloatSliderParameterNoInterpolation gridScale = new(3.0f, 1.0f, 20.0f);

    /// <summary> Height calculation method. Default Depth. </summary>
    [EnumDropdown(1, "Height calculation method. Default Depth.")]
    public EnumParameterNoInterpolation<HeightMethod> heightMethod = new(HeightMethod.Depth);

    /// <summary> Depth scale for height calculation [0.1, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Depth scale for height calculation [0.1, 10]. Default 1.")]
    public FloatSliderParameterNoInterpolation depthScale = new(1.0f, 0.1f, 10.0f);

    /// <summary> Depth remap minimum [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Depth remap minimum [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation depthRemapMin = new(0.0f, 0.0f, 1.0f);

    /// <summary> Depth remap maximum [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Depth remap maximum [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation depthRemapMax = new(1.0f, 0.0f, 1.0f);

    /// <summary> Luminosity remap minimum [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Luminosity remap minimum [0, 1]. Default 0.")]
    public FloatSliderParameterNoInterpolation luminosityRemapMin = new(0.0f, 0.0f, 1.0f);

    /// <summary> Luminosity remap maximum [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Luminosity remap maximum [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation luminosityRemapMax = new(1.0f, 0.0f, 1.0f);

    /// <summary> Maximum ray distance [1, 50]. Default 20. </summary>
    [FloatSliderWithReset(20.0f, 1.0f, 50.0f, "Maximum ray distance [1, 50]. Default 20.")]
    public FloatSliderParameterNoInterpolation maxRayDistance = new(20.0f, 1.0f, 50.0f);

    /// <summary> Raymarching step multiplier [0.1, 1]. Default 0.7. </summary>
    [FloatSliderWithReset(0.7f, 0.1f, 1.0f, "Raymarching step multiplier [0.1, 1]. Default 0.7.")]
    public FloatSliderParameterNoInterpolation stepMultiplier = new(0.7f, 0.1f, 1.0f);

    /// <summary> Shadow softness [1, 16]. Default 8. </summary>
    [FloatSliderWithReset(8.0f, 1.0f, 16.0f, "Shadow softness [1, 16]. Default 8.")]
    public FloatSliderParameterNoInterpolation shadowSoftness = new(8.0f, 1.0f, 16.0f);

    /// <summary> Raymarching steps [4, 128]. Default 32. </summary>
    [IntSliderWithReset(32, 4, 128, "Raymarching steps [4, 128]. Default 32.")]
    public ClampedIntParameterNoInterpolation raymarchingSteps = new(32, 4, 128);

    /// <summary> Color blend. Default Solid. </summary>
    [EnumDropdown(16, "Color blend. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> colorBlend = new(ColorBlends.Solid);

    /// <summary> Camera distance from the scene [-5, -0.5]. Default -2. </summary>
    [FloatSliderWithReset(-2.0f, -5.0f, -0.5f, "Camera distance from the scene [-5, -0.5]. Default -2.")]
    public FloatSliderParameterNoInterpolation cameraDistance = new(-2.0f, -5.0f, -0.5f);

    /// <summary> Rotation [0, 360]. Default 0. </summary>
    [Vector2WithReset(0.0f, 0.0f, "Rotation [0, 360]. Default 0.")]
    public Vector2ParameterNoInterpolation rotation = new(Vector2.zero);

    /// <summary> Light position [-3, 3]. Default (1.5, 2.0, -1.0). </summary>
    [Vector3WithReset(1.5f, 2.0f, -1.0f, "Light position [-3, 3]. Default (1.5, 2.0, -1.0).")]
    public Vector3ParameterNoInterpolation lightPosition = new(new Vector3(1.5f, 2.0f, -1.0f));

    /// <summary> Light color. Default (0.25, 0.5, 1.0, 0.3). </summary>
    [ColorWithReset(0x4080FF4D, "Light color. Default (0.25, 0.5, 1.0, 0.3).")]
    public ColorParameterNoInterpolation lightColor = new(new Color(0.25f, 0.5f, 1.0f, 0.3f));

    /// <summary> Specular color. Default (1.0, 0.5, 0.2). </summary>
    [ColorWithReset(0xFF8033FF, "Specular color. Default (1.0, 0.5, 0.2).")]
    public ColorParameterNoInterpolation specularColor = new(new Color(1.0f, 0.5f, 0.2f));

    /// <summary> Fresnel intensity [0, 128]. Default 16. </summary>
    [FloatSliderWithReset(16.0f, 0.0f, 128.0f, "Fresnel intensity [0, 128]. Default 16.")]
    public FloatSliderParameterNoInterpolation fresnelIntensity = new(16.0f, 0.0f, 128.0f);

    /// <summary> Shadow iterations [0, 32]. Default 8. </summary>
    [IntSliderWithReset(8, 0, 32, "Shadow iterations [0, 32]. Default 8.")]
    public ClampedIntParameterNoInterpolation shadowIterations = new(8, 0, 32);

    /// <summary> Ambient occlusion iterations [0, 32]. Default 5. </summary>
    [IntSliderWithReset(5, 0, 32, "Ambient occlusion iterations [0, 32]. Default 5.")]
    public ClampedIntParameterNoInterpolation ambientOcclusionIterations = new(5, 0, 32);

    /// <summary> Floor color. Default black. </summary>
    [ColorWithReset(0x000000FF, "Floor color. Default black.")]
    public ColorParameterNoInterpolation floorColor = new(Color.black);

    /// <summary> Floor color blend. Default Solid. </summary>
    [EnumDropdown(16, "Floor color blend. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> floorColorBlend = new(ColorBlends.Solid);

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

      gridScale.value = 3.0f;
      heightMethod.value = HeightMethod.Depth;
      depthScale.value = 1.0f;
      depthRemapMin.value = 0.0f;
      depthRemapMax.value = 1.0f;
      luminosityRemapMin.value = 0.0f;
      luminosityRemapMax.value = 1.0f;
      maxRayDistance.value = 20.0f;
      stepMultiplier.value = 0.7f;
      shadowSoftness.value = 8.0f;
      raymarchingSteps.value = 32;
      colorBlend.value = ColorBlends.Solid;
      cameraDistance.value = -2.0f;
      rotation.value = Vector2.zero;
      lightPosition.value = new Vector3(1.5f, 2.0f, -1.0f);
      lightColor.value = new Color(0.25f, 0.5f, 1.0f, 0.3f);
      specularColor.value = new Color(1.0f, 0.5f, 0.2f);
      fresnelIntensity.value = 16.0f;
      shadowIterations.value = 8;
      ambientOcclusionIterations.value = 5;
      floorColor.value = Color.black;
      floorColorBlend.value = ColorBlends.Solid;

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
