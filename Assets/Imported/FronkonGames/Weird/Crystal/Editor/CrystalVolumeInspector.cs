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

namespace FronkonGames.Weird.Crystal.Editor
{
  /// <summary> Crystal Volume inspector. </summary>
  [CustomEditor(typeof(CrystalVolume))]
  public class CrystalVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Crystal.
      /////////////////////////////////////////////////
      Separator();

      DrawFloatSliderWithReset("crystalIntensity", "Crystal");
      IndentLevel++;
      DrawEnumDropdownWithReset("crystalColorBlend", "Blend", ColorBlends.Additive);
      DrawVector3WithReset("crystalColor", "Color", CrystalVolume.DefaultCrystalColor);
      DrawFloatSliderWithReset("crystalGain", "Gain");
      DrawFloatSliderWithReset("crystalScale", "Scale");
      DrawFloatSliderWithReset("crystalSpeed", "Speed");
      DrawFloatSliderWithReset("crystalPower", "Power");
      DrawFloatSliderWithReset("crystalRotation0", "Rotation #0");
      DrawFloatSliderWithReset("crystalRotation1", "Rotation #1");
      DrawFloatSliderWithReset("crystalReflection", "Reflection");
      IndentLevel++;
      DrawFloatSliderWithReset("crystalRefraction", "Refraction");
      IndentLevel--;
      IndentLevel--;

      DrawFloatSliderWithReset("lightsIntensity", "Lights");
      IndentLevel++;
      DrawEnumDropdownWithReset("lightsColorBlend", "Blend", ColorBlends.Screen);
      DrawFloatSliderWithReset("lightsSpeed", "Speed");
      DrawIntSliderWithReset("lightsIterations", "Iterations");
      DrawVector3WithReset("lightsColorOffset", "Color Offset", CrystalVolume.DefaultLightsColorOffset);
      DrawFloatSliderWithReset("lightsComplexity", "Complexity");
      DrawFloatSliderWithReset("lightsDistortion", "Distortion");
      DrawFloatSliderWithReset("lightsSpread", "Spread");
      DrawFloatSliderWithReset("lightsRotationSpeed", "Rotation Speed");
      DrawFloatSliderWithReset("lightsTurbulence", "Turbulence");
      DrawFloatSliderWithReset("lightsDetail", "Detail");
      DrawFloatSliderWithReset("lightsWarp", "Warp");
      DrawFloatSliderWithReset("lightsBrightness", "Brightness");
      DrawFloatSliderWithReset("lightsContrast", "Contrast");
      DrawFloatSliderWithReset("lightsPower", "Power");
      IndentLevel--;
    }

    protected override void ResetValues() => ((CrystalVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (Crystal.IsInAnyRenderFeatures() == false)
      {
        Separator();
        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        Crystal[] effects = Crystal.Instances;
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
