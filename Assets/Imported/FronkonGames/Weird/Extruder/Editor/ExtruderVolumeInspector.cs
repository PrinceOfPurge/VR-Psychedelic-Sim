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

namespace FronkonGames.Weird.Extruder.Editor
{
  /// <summary> Extruder Volume inspector. </summary>
  [CustomEditor(typeof(ExtruderVolume))]
  public class ExtruderVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      ExtruderVolume volume = (ExtruderVolume)target;

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      Separator();

      /////////////////////////////////////////////////
      // Extruder.
      /////////////////////////////////////////////////
      Label("Extruder");
      IndentLevel++;
      DrawFloatSliderWithReset("gridScale", "Scale");
      DrawEnumDropdownWithReset("heightMethod", "Height Method", HeightMethod.Depth);
      if (volume.heightMethod.value == HeightMethod.Depth)
      {
        DrawFloatSliderWithReset("depthScale", "Scale");
        DrawMinMaxSliderWithReset("depthRemapMin", "depthRemapMax", "Remap", 0.0f, 1.0f);
      }
      else
        DrawMinMaxSliderWithReset("luminosityRemapMin", "luminosityRemapMax", "Remap", 0.0f, 1.0f);
      DrawEnumDropdownWithReset("colorBlend", "Blend", ColorBlends.Solid);
      DrawVector2WithReset("rotation", "Rotation", Vector2.zero);
      IndentLevel--;

      Label("Camera & Lighting");
      IndentLevel++;
      DrawFloatSliderWithReset("cameraDistance", "Distance");
      DrawVector3WithReset("lightPosition", "Light Position", new Vector3(1.5f, 2.0f, -1.0f));
      IndentLevel++;
      DrawColorWithReset("lightColor", "Color", new Color(0.25f, 0.5f, 1.0f, 0.3f));
      DrawColorWithReset("specularColor", "Specular", new Color(1.0f, 0.5f, 0.2f));
      DrawFloatSliderWithReset("fresnelIntensity", "Fresnel");
      IndentLevel--;
      IndentLevel--;

      Label("Raymarching");
      IndentLevel++;
      DrawFloatSliderWithReset("maxRayDistance", "Max Ray Distance");
      DrawIntSliderWithReset("raymarchingSteps", "Steps");
      DrawFloatSliderWithReset("stepMultiplier", "Step Multiplier");
      DrawFloatSliderWithReset("shadowSoftness", "Shadow Softness");
      DrawIntSliderWithReset("shadowIterations", "Shadow Iterations");
      DrawIntSliderWithReset("ambientOcclusionIterations", "AO Iterations");
      IndentLevel--;

      Label("Floor");
      IndentLevel++;
      DrawColorWithReset("floorColor", "Color", Color.black);
      DrawEnumDropdownWithReset("floorColorBlend", "Blend", ColorBlends.Solid);
      IndentLevel--;
    }

    protected override void ResetValues() => ((ExtruderVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Extruder.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Extruder[] effects = Extruder.Instances;
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
