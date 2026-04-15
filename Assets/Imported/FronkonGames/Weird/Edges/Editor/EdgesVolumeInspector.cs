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

namespace FronkonGames.Weird.Edges.Editor
{
  /// <summary> Edges Volume inspector. </summary>
  [CustomEditor(typeof(EdgesVolume))]
  public class EdgesVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Edges.
      /////////////////////////////////////////////////
      Separator();

      DrawFloatSliderWithReset("edges");
      IndentLevel++;
      DrawEnumDropdownWithReset("edgeDetectionMethod", "Method", EdgeDetectionMethod.DepthNeighbors);
      DrawIntSliderWithReset("edgeWidth", "Width");
      DrawFloatSliderWithReset("edgeThreshold", "Threshold");
      DrawEnumDropdownWithReset("edgeColorMode", "Color mode", EdgeColorMode.Linear);
      IndentLevel++;
      if (((EdgesVolume)target).edgeColorMode.value == EdgeColorMode.Linear)
      {
        DrawColorWithReset("edgesColor", "Color", Color.white);
      }
      else if (((EdgesVolume)target).edgeColorMode.value == EdgeColorMode.DepthBased)
      {
        DrawColorWithReset("depthEdgeNearColor", "Near color", Color.cyan);
        DrawColorWithReset("depthEdgeFarColor", "Far color", Color.magenta);
        DrawFloatSliderWithReset("depthEdgeNear", "Near");
        DrawFloatSliderWithReset("depthEdgeFar", "Far");
      }
      DrawEnumDropdownWithReset("edgesBlend", "Blend", ColorBlends.Solid);
      IndentLevel--;
      IndentLevel--;

      DrawColorWithReset("backgroundColor", "Background", Color.black);
      IndentLevel++;
      DrawEnumDropdownWithReset("backgroundBlend", "Blend", ColorBlends.Solid);
      IndentLevel--;

      DrawToggleWithReset("ignoreSkybox", true);
    }

    protected override void ResetValues() => ((EdgesVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Edges.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Edges[] effects = Edges.Instances;
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
