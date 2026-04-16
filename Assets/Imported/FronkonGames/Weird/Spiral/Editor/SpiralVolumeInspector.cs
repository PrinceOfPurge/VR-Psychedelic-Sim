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

namespace FronkonGames.Weird.Spiral.Editor
{
  /// <summary> Spiral Volume inspector. </summary>
  [CustomEditor(typeof(SpiralVolume))]
  public class SpiralVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Spiral.
      /////////////////////////////////////////////////
      Separator();

      DrawFloatSliderWithReset("wrap");
      IndentLevel++;
      DrawEnumDropdownWithReset("shape", "Shape", ShapeType.Circular);
      DrawVector2WithReset("center", "Center", Vector2.zero);
      DrawFloatSliderWithReset("spiralAmount", "Spiral Amount");
      DrawFloatSliderWithReset("rotation", "Rotation");
      DrawFloatSliderWithReset("rotationSpeed", "Rotation Speed");
      DrawFloatSliderWithReset("outerRing", "Outer Ring");
      DrawFloatSliderWithReset("zoomSpeed", "Zoom Speed");
      DrawFloatSliderWithReset("frequency", "Frequency");
      DrawEnumDropdownWithReset("edgeMode", "Edge Mode", EdgeMode.Mirror);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Outer Tint.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("outerTintIntensity", "Outer Tint");
      IndentLevel++;
      DrawEnumDropdownWithReset("outerTintColorBlend", "Blend", ColorBlends.Solid);
      DrawColorWithReset("outerTintColor", "Color", Color.cyan);
      DrawFloatSliderWithReset("outerTintSoftness", "Softness");
      IndentLevel--;

      /////////////////////////////////////////////////
      // Shadow.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("shadowIntensity", "Shadow");
      IndentLevel++;
      DrawEnumDropdownWithReset("shadowColorBlend", "Blend", ColorBlends.Multiply);
      DrawColorWithReset("shadowColor", "Color", Color.black);
      DrawFloatSliderWithReset("shadowSoftness", "Softness");
      DrawFloatSliderWithReset("shadowOffset", "Offset");
      IndentLevel--;

      /////////////////////////////////////////////////
      // Line.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("lineWidth", "Line");
      IndentLevel++;
      DrawEnumDropdownWithReset("lineColorBlend", "Blend", ColorBlends.Solid);
      DrawColorWithReset("lineColor", "Color", Color.white);
      DrawFloatSliderWithReset("lineSoftness", "Softness");
      DrawIntSliderWithReset("lineCount", "Count");
      IndentLevel--;
    }

    protected override void ResetValues() => ((SpiralVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Spiral.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Spiral[] effects = Spiral.Instances;
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
