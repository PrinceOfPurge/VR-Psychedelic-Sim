using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Crystal;

/// <summary> Weird: Crystal demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class CrystalDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private CrystalVolume volume;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private Vector2 scrollPosition;

  private void ResetEffect()
  {
    volume.Reset();
    volume.intensity.value = 1.0f;
  }

  private void Awake()
  {
    styleTitle = styleLabel = styleButton = null;

    if (Crystal.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out CrystalVolume vol) ? vol : null;
    this.enabled = Crystal.IsInRenderFeatures() && volume != null;
  }

  private void Start() => ResetEffect();

  private void OnGUI()
  {
    styleTitle ??= new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.LowerCenter,
      fontSize = 32,
      fontStyle = FontStyle.Bold
    };

    styleLabel ??= new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.UpperLeft,
      fontSize = 24
    };

    styleButton ??= new GUIStyle(GUI.skin.button)
    {
      fontSize = 24
    };

    GUILayout.BeginHorizontal();
    {
      GUILayout.BeginVertical("box", GUILayout.Width(450.0f), GUILayout.Height(Screen.height));
      {
        const float space = 10.0f;

        GUILayout.Space(space);

        GUILayout.Label("CRYSTAL DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value);

        GUILayout.Space(5.0f);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        {
          // Crystal settings.
          volume.crystalIntensity.value = SliderField("Crystal", volume.crystalIntensity.value);
          volume.crystalColorBlend.value = EnumField("  Blend", volume.crystalColorBlend.value);
          volume.crystalColor.value = Vector3Field("  Color", volume.crystalColor.value, "R", "G", "B", 0.0f, 2.0f);
          volume.crystalGain.value = SliderField("  Gain", volume.crystalGain.value, 0.0f, 2.0f);
          volume.crystalScale.value = SliderField("  Scale", volume.crystalScale.value, 0.1f, 10.0f);
          volume.crystalSpeed.value = SliderField("  Speed", volume.crystalSpeed.value, 0.0f, 2.0f);
          volume.crystalPower.value = SliderField("  Power", volume.crystalPower.value, 0.1f, 20.0f);
          volume.crystalRotation0.value = SliderField("  Rotation #0", volume.crystalRotation0.value, 0.0f, 180.0f);
          volume.crystalRotation1.value = SliderField("  Rotation #1", volume.crystalRotation1.value, 0.0f, 180.0f);
          volume.crystalReflection.value = SliderField("  Reflection", volume.crystalReflection.value, 0.0f, 1.0f);
          volume.crystalRefraction.value = SliderField("  Refraction", volume.crystalRefraction.value, 0.0f, 10.0f);

          GUILayout.Space(5.0f);

          // Lights settings.
          volume.lightsIntensity.value = SliderField("Lights", volume.lightsIntensity.value);
          volume.lightsColorBlend.value = EnumField("  Blend", volume.lightsColorBlend.value);
          volume.lightsSpeed.value = SliderField("  Speed", volume.lightsSpeed.value, 0.0f, 2.0f);
          volume.lightsIterations.value = SliderField("  Iterations", volume.lightsIterations.value, 5, 30);
          volume.lightsColorOffset.value = Vector3Field("  Color Offset", volume.lightsColorOffset.value, "R", "G", "B", 0.0f, 5.0f);
          volume.lightsComplexity.value = SliderField("  Complexity", volume.lightsComplexity.value, 0.01f, 0.1f);
          volume.lightsDistortion.value = SliderField("  Distortion", volume.lightsDistortion.value, 1.0f, 15.0f);
          volume.lightsSpread.value = SliderField("  Spread", volume.lightsSpread.value, 1.0f, 10.0f);
          volume.lightsRotationSpeed.value = SliderField("  Rotation Speed", volume.lightsRotationSpeed.value, 0.0f, 0.1f);
          volume.lightsTurbulence.value = SliderField("  Turbulence", volume.lightsTurbulence.value, 10.0f, 100.0f);
          volume.lightsDetail.value = SliderField("  Detail", volume.lightsDetail.value, 0.5f, 3.0f);
          volume.lightsWarp.value = SliderField("  Warp", volume.lightsWarp.value, 3.0f, 15.0f);
          volume.lightsBrightness.value = SliderField("  Brightness", volume.lightsBrightness.value, 5.0f, 50.0f);
          volume.lightsContrast.value = SliderField("  Contrast", volume.lightsContrast.value, 5.0f, 30.0f);
          volume.lightsPower.value = SliderField("  Power", volume.lightsPower.value, 1.0f, 10.0f);

          GUILayout.Space(5.0f);

          // Color settings.
          volume.brightness.value = SliderField("Brightness", volume.brightness.value, -1.0f, 1.0f);
          volume.contrast.value = SliderField("Contrast", volume.contrast.value, 0.0f, 10.0f);
          volume.gamma.value = SliderField("Gamma", volume.gamma.value, 0.1f, 10.0f);
          volume.hue.value = SliderField("Hue", volume.hue.value, 0.0f, 1.0f);
          volume.saturation.value = SliderField("Saturation", volume.saturation.value, 0.0f, 2.0f);
        }
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("RESET", styleButton) == true)
          ResetEffect();

        GUI.enabled = true;

        GUILayout.Space(4.0f);

        if (GUILayout.Button("ONLINE DOCUMENTATION", styleButton) == true)
          Application.OpenURL(Constants.Support.Documentation);

        GUILayout.Space(4.0f);

        if (GUILayout.Button("❤️ LEAVE A REVIEW ❤️", styleButton) == true)
          Application.OpenURL(Constants.Support.Store);

        GUILayout.Space(space * 2.0f);
      }
      GUILayout.EndVertical();

      GUILayout.FlexibleSpace();
    }
    GUILayout.EndHorizontal();
  }

  private void OnDestroy()
  {
    ResetEffect();
  }

  private bool ToggleField(string label, bool value)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.Toggle(value, string.Empty);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private float SliderField(string label, float value, float min = 0.0f, float max = 1.0f)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private int SliderField(string label, int value, int min, int max)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = (int)GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Color ColorField(string label, Color value, bool alpha = true)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      float originalAlpha = value.a;

      UnityEngine.Color.RGBToHSV(value, out float h, out float s, out float v);
      h = GUILayout.HorizontalSlider(h, 0.0f, 1.0f);
      value = UnityEngine.Color.HSVToRGB(h, s, v);

      if (alpha == false)
        value.a = originalAlpha;
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Vector2 Vector2Field(string label, Vector2 value, string x = "X", string y = "Y", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);

    return value;
  }

  private Vector3 Vector3Field(string label, Vector3 value, string x = "X", string y = "Y", string z = "Z", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);
    value.z = SliderField($"   {z}", value.z, min, max);

    return value;
  }

  private T EnumField<T>(string label, T value) where T : Enum
  {
    string[] names = System.Enum.GetNames(typeof(T));
    Array values = System.Enum.GetValues(typeof(T));
    int index = Array.IndexOf(values, value);

    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      if (GUILayout.Button("<", styleButton) == true)
        index = index > 0 ? index - 1 : values.Length - 1;

      GUILayout.Label(names[index], styleLabel);

      if (GUILayout.Button(">", styleButton) == true)
        index = index < values.Length - 1 ? index + 1 : 0;
    }
    GUILayout.EndHorizontal();

    return (T)(object)index;
  }
}
