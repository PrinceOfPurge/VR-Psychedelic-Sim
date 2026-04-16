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

namespace FronkonGames.Weird.Kaleidoscope.Editor
{
  /// <summary> Kaleidoscope Volume inspector. </summary>
  [CustomEditor(typeof(KaleidoscopeVolume))]
  public class KaleidoscopeVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      Separator();
      
      /////////////////////////////////////////////////
      // Kaleidoscope.
      /////////////////////////////////////////////////
      Label("Kaleidoscope");
      IndentLevel++;
      DrawVector2WithReset("center", "Center", new Vector2(0.5f, 0.5f));
      DrawIntSliderWithReset("iterationCount", "Iterations");
      DrawCurveWithReset("strength", "Strength", KaleidoscopeVolume.DefaultStrength);
      DrawFloatSliderWithReset("speed", "Speed");
      DrawFloatSliderWithReset("scale", "Scale");
      DrawToggleWithReset("keepAspectRatio", false);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Offset UV.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("offsetIntensity", "Offset UV");
      IndentLevel++;
      DrawVector2WithReset("offsetRedScale", "Red", new Vector2(10.0f, 10.0f));
      DrawVector2WithReset("offsetGreenScale", "Green", new Vector2(-10.0f, 10.0f));
      DrawVector2WithReset("offsetBlueScale", "Blue", new Vector2(10.0f, -10.0f));
      DrawFloatSliderWithReset("offsetScale", "Scale");
      IndentLevel--;

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("colorIntensity", "Color");
      IndentLevel++;
      DrawEnumDropdownWithReset("colorPalette", "Palette", ColorPalettes.Original);
      DrawEnumDropdownWithReset("blend", "Blend", ColorBlends.Additive);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Segment.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("segmentIntensity", "Segment");
      IndentLevel++;
      DrawEnumDropdownWithReset("segmentBlend", "Blend", ColorBlends.Solid);
      DrawColorWithReset("segmentColor", "Color", Color.black);
      DrawFloatSliderWithReset("segmentWidth", "Width");
      IndentLevel--;
    }

    protected override void ResetValues() => ((KaleidoscopeVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Kaleidoscope.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Kaleidoscope[] effects = Kaleidoscope.Instances;
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
