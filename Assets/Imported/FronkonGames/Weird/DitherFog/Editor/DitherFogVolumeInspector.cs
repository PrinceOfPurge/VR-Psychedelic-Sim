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
using UnityEditor;
using UnityEngine;

namespace FronkonGames.Weird.DitherFog.Editor
{
  /// <summary> Dither Fog Volume inspector. </summary>
  [CustomEditor(typeof(DitherFogVolume))]
  public class DitherFogVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Dither Fog.
      /////////////////////////////////////////////////
      Separator();

      DrawFloatSliderWithReset("fogOpacity", "Fog");
      IndentLevel++;
      DrawEnumDropdownWithReset("fogColorMode", "Color Mode", FogColorModes.Solid);
      DrawColorWithReset("fogColor", "Color", Color.black);
      DrawColorWithReset("fogGradientNear", "Gradient Near", new Color(0.0f, 0.0f, 0.0f, 0.0f));
      DrawColorWithReset("fogGradientFar", "Gradient Far", new Color(0.0f, 0.0f, 0.0f, 1.0f));
      DrawFloatSliderWithReset("fogStart", "Start");
      DrawFloatSliderWithReset("fogCurveStart", "Curve Start");
      DrawFloatSliderWithReset("fogCurveEnd", "Curve End");
      DrawToggleWithReset("curvedFog", true);
      IndentLevel--;

      DrawEnumDropdownWithReset("ditheringMode", "Dithering", DitheringModes.Bayer8x8);
      IndentLevel++;
      DrawIntSliderWithReset("ditherScale", "Scale");
      DrawToggleWithReset("adaptiveDithering", false);
      IndentLevel++;
      DrawFloatSliderWithReset("adaptiveBrightnessThreshold", "Brightness Threshold");
      DrawFloatSliderWithReset("adaptiveContrastSensitivity", "Contrast Sensitivity");
      IndentLevel--;
      DrawIntSliderWithReset("quantize", "Quantize");
      IndentLevel--;
    }

    protected override void ResetValues() => ((DitherFogVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (DitherFog.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        DitherFog[] effects = DitherFog.Instances;
        bool anyEnabled = false;
        for (int i = 0; i < effects.Length; i++)
        {
          if (effects[i].isActive == true)
          {
            anyEnabled = true;
            break;
          }
        }

        if (anyEnabled == false)
        {
          Separator();
          EditorGUILayout.HelpBox($"No Renderer Feature '{Constants.Asset.Name}' is active. You must activate it in the Render Features.", MessageType.Warning);
        }
      }
    }
  }
}
