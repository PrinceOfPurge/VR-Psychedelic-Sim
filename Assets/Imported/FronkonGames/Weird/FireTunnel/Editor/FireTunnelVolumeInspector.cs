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

namespace FronkonGames.Weird.FireTunnel.Editor
{
  /// <summary> Fire Tunnel Volume inspector. </summary>
  [CustomEditor(typeof(FireTunnelVolume))]
  public class FireTunnelVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      Separator();

      /////////////////////////////////////////////////
      // Fire Tunnel.
      /////////////////////////////////////////////////
      Label("Tunnel");
      IndentLevel++;
      DrawVector2WithReset("center", "Center", Vector2.zero);
      DrawFloatSliderWithReset("tunnelRadius", "Radius");
      DrawFloatSliderWithReset("turbulence", "Turbulence");
      IndentLevel--;

      Label("Animation");
      IndentLevel++;
      DrawFloatSliderWithReset("speed", "Speed");
      DrawFloatSliderWithReset("rotation", "Rotation");
      IndentLevel--;

      Label("Color");
      IndentLevel++;
      DrawEnumDropdownWithReset("colorBlend", "Blend", ColorBlends.Additive);
      DrawFloatSliderWithReset("fireIntensity", "Fire Intensity");
      DrawVector3WithReset("fireColor", "Fire Color", new Vector3(5.0f, 2.0f, 1.0f));
      IndentLevel--;

      Label("Quality");
      IndentLevel++;
      DrawIntSliderWithReset("raymarchSteps", "Steps");
      DrawFloatSliderWithReset("noiseScale", "Noise Scale");
      IndentLevel--;
    }

    protected override void ResetValues() => ((FireTunnelVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (FireTunnel.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        FireTunnel[] effects = FireTunnel.Instances;
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
