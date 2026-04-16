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

namespace FronkonGames.Weird.Pinch.Editor
{
  /// <summary> Pinch Volume inspector. </summary>
  [CustomEditor(typeof(PinchVolume))]
  public class PinchVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      Separator();

      /////////////////////////////////////////////////
      // Pinch.
      /////////////////////////////////////////////////
      
      Label("Pinch");
      IndentLevel++;  
      DrawVector2WithReset("start", "Start", new Vector2(0.5f, 0.5f));
      DrawVector2WithReset("end", "End", Vector2.zero);
      DrawFloatSliderWithReset("startRadius", "Start Radius");
      DrawFloatSliderWithReset("endRadius", "End Radius");
      DrawFloatSliderWithReset("roundness", "Roundness");
      IndentLevel--;

      DrawFloatSliderWithReset("lightIntensity", "Lighting");
      IndentLevel++;
      DrawVector3WithReset("lightDirection", "Direction", new Vector3(0.577f, 0.577f, 0.577f));
      IndentLevel--;

      Label("Raymarching");
      IndentLevel++;
      DrawFloatSliderWithReset("raymarchSteps", "Raymarch Steps");
      DrawFloatSliderWithReset("maxDistance", "Max Distance");
      IndentLevel--;
    }

    protected override void ResetValues() => ((PinchVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Pinch.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Pinch[] effects = Pinch.Instances;
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
