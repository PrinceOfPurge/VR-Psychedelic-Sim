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

namespace FronkonGames.Weird.Bubbles.Editor
{
  /// <summary> Bubbles Volume inspector. </summary>
  [CustomEditor(typeof(BubblesVolume))]
  public class BubblesVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Bubbles.
      /////////////////////////////////////////////////
      Separator();

      DrawColorWithReset("bubbleColor", "Bubbles", Color.white);
      IndentLevel++;
      DrawEnumDropdownWithReset("bubbleColorBlend", "Blend", ColorBlends.Solid);
      DrawFloatSliderWithReset("bubbleRoundness", "Roundness");
      DrawFloatSliderWithReset("bubbleSize", "Size");
      DrawFloatSliderWithReset("bubbleBevel", "Bevel");
      DrawFloatSliderWithReset("bubbleSpacing", "Spacing");
      IndentLevel--;

      DrawFloatSliderWithReset("lightSpecular", "Lighting");
      IndentLevel++;
      DrawColorWithReset("lightColor", "Color", Color.white);
      DrawFloatSliderWithReset("lightSpecularPower", "Power");
      DrawFloatSliderWithReset("lightAngle", "Angle");
      DrawFloatSliderWithReset("lightElevation", "Elevation");
      IndentLevel--;

      DrawColorWithReset("backgroundColor", "Background", Color.white);
      IndentLevel++;
      DrawEnumDropdownWithReset("backgroundBlend", "Blend", ColorBlends.Solid);
      DrawFloatSliderWithReset("backgroundBlur", "Blur");
      DrawFloatSliderWithReset("backgroundExposure", "Exposure");
      IndentLevel--;
    }

    protected override void ResetValues() => ((BubblesVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Bubbles.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Bubbles[] effects = Bubbles.Instances;
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