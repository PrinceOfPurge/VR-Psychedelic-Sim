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

namespace FronkonGames.Weird.Edges
{
  /// <summary> Edges Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Weird/Edges"), HelpURL(Constants.Support.Documentation)]
  public sealed class EdgesVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public FloatSliderParameterLinear intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Edges settings.

    /// <summary> Edge detection strength [0, 1]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Edge detection strength [0, 1]. Default 1.")]
    public FloatSliderParameterNoInterpolation edges = new(1.0f, 0.0f, 1.0f);

    /// <summary> Edge width multiplier [1, 10]. Default 2. </summary>
    [IntSliderWithReset(2, 1, 10, "Edge width multiplier [1, 10]. Default 2.")]
    public ClampedIntParameterNoInterpolation edgeWidth = new(2, 1, 10);

    /// <summary> Edge detection threshold [0.01, 1.0]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.01f, 1.0f, "Edge detection threshold [0.01, 1.0]. Default 0.1.")]
    public FloatSliderParameterNoInterpolation edgeThreshold = new(0.1f, 0.01f, 1.0f);

    /// <summary> Edge detection method. Default DepthNeighbors. </summary>
    [EnumDropdown(0, "Edge detection method. Default DepthNeighbors.")]
    public EnumParameterNoInterpolation<EdgeDetectionMethod> edgeDetectionMethod = new(EdgeDetectionMethod.DepthNeighbors);

    /// <summary> Edge color mode. Default Linear. </summary>
    [EnumDropdown(0, "Edge color mode. Default Linear.")]
    public EnumParameterNoInterpolation<EdgeColorMode> edgeColorMode = new(EdgeColorMode.Linear);

    /// <summary> Color for the detected edges. Default white. </summary>
    [ColorWithReset(0xFFFFFFFF, "Color for the detected edges. Default white.")]
    public ColorParameterNoInterpolation edgesColor = new(Color.white);

    /// <summary> Color blend for the detected edges. Default Solid. </summary>
    [EnumDropdown((int)ColorBlends.Solid, "Color blend for the detected edges. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> edgesBlend = new(ColorBlends.Solid);

    /// <summary> Background color. Default black. </summary>
    [ColorWithReset(0xFF000000, "Background color. Default black.")]
    public ColorParameterNoInterpolation backgroundColor = new(Color.black);

    /// <summary> Color blend for the background. Default Solid. </summary>
    [EnumDropdown((int)ColorBlends.Solid, "Color blend for the background. Default Solid.")]
    public EnumParameterNoInterpolation<ColorBlends> backgroundBlend = new(ColorBlends.Solid);

    /// <summary> Near edge color (close to camera). Default cyan. </summary>
    [ColorWithReset(0x00FFFFFF, "Near edge color (close to camera). Default cyan.")]
    public ColorParameterNoInterpolation depthEdgeNearColor = new(Color.cyan);

    /// <summary> Far edge color (away from camera). Default magenta. </summary>
    [ColorWithReset(0xFF00FFFF, "Far edge color (away from camera). Default magenta.")]
    public ColorParameterNoInterpolation depthEdgeFarColor = new(Color.magenta);

    /// <summary> Depth range start (near) [0.0, 1.0]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "Depth range start (near) [0.0, 1.0]. Default 0.")]
    public FloatSliderParameterNoInterpolation depthEdgeNear = new(0.0f, 0.0f, 1.0f);

    /// <summary> Depth range end (far) [0.0, 1.0]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Depth range end (far) [0.0, 1.0]. Default 1.")]
    public FloatSliderParameterNoInterpolation depthEdgeFar = new(1.0f, 0.0f, 1.0f);

    /// <summary> Ignore skybox. Default true. </summary>
    [ToggleWithReset(true, "Ignore skybox. Default true.")]
    public BoolParameterNoInterpolation ignoreSkybox = new(true);

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

      edges.value = 1.0f;
      edgeWidth.value = 2;
      edgeThreshold.value = 0.1f;
      edgeDetectionMethod.value = EdgeDetectionMethod.DepthNeighbors;
      edgeColorMode.value = EdgeColorMode.Linear;
      edgesColor.value = Color.white;
      edgesBlend.value = ColorBlends.Solid;
      backgroundColor.value = Color.black;
      backgroundBlend.value = ColorBlends.Solid;
      depthEdgeNearColor.value = Color.cyan;
      depthEdgeFarColor.value = Color.magenta;
      depthEdgeNear.value = 0.0f;
      depthEdgeFar.value = 1.0f;
      ignoreSkybox.value = true;

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
