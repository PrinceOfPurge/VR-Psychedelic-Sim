using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Extruder;

/// <summary> Weird: Extruder demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class ExtruderDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private ExtruderVolume volume;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void ResetEffect()
  {
    volume.Reset();
    volume.intensity.value = 1.0f;
    volume.gamma.value = 1.75f;
  }

  private void Awake()
  {
    styleTitle = styleLabel = styleButton = null;

    if (Extruder.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out ExtruderVolume vol) ? vol : null;
    this.enabled = Extruder.IsInRenderFeatures() && volume != null;
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

        GUILayout.Label("EXTRUDER DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value);

        // Extruder settings.
        volume.gridScale.value = SliderField("Grid Scale", volume.gridScale.value, 1.0f, 20.0f);
        volume.heightMethod.value = EnumField("Height Method", volume.heightMethod.value);
        volume.depthScale.value = SliderField("Depth Scale", volume.depthScale.value, 0.1f, 10.0f);
        volume.depthRemapMin.value = SliderField("Depth Min", volume.depthRemapMin.value, 0.0f, 1.0f);
        volume.depthRemapMax.value = SliderField("Depth Max", volume.depthRemapMax.value, 0.0f, 1.0f);
        volume.luminosityRemapMin.value = SliderField("Lum Min", volume.luminosityRemapMin.value, 0.0f, 1.0f);
        volume.luminosityRemapMax.value = SliderField("Lum Max", volume.luminosityRemapMax.value, 0.0f, 1.0f);
        volume.colorBlend.value = EnumField("Color Blend", volume.colorBlend.value);
        volume.rotation.value = Vector2Field("Rotation", volume.rotation.value, "Horizontal", "Vertical", 0.0f, 360.0f);
        volume.cameraDistance.value = SliderField("Camera Distance", volume.cameraDistance.value, -5.0f, -0.5f);
        volume.lightPosition.value = Vector3Field("Light Position", volume.lightPosition.value, "X", "Y", "Z", -3.0f, 3.0f);
        volume.lightColor.value = ColorField("Light Color", volume.lightColor.value, false);
        volume.specularColor.value = ColorField("Specular Color", volume.specularColor.value, false);
        volume.fresnelIntensity.value = SliderField("Fresnel Intensity", volume.fresnelIntensity.value, 0.0f, 128.0f);
        volume.shadowIterations.value = SliderField("Shadow Iterations", volume.shadowIterations.value, 0, 32);
        volume.ambientOcclusionIterations.value = SliderField("AO Iterations", volume.ambientOcclusionIterations.value, 0, 32);
        volume.floorColor.value = ColorField("Floor Color", volume.floorColor.value, false);
        volume.floorColorBlend.value = EnumField("Floor Blend", volume.floorColorBlend.value);

        GUILayout.Space(5.0f);

        volume.brightness.value = SliderField("Brightness", volume.brightness.value, -1.0f, 1.0f);
        volume.contrast.value = SliderField("Contrast", volume.contrast.value, 0.0f, 10.0f);
        volume.gamma.value = SliderField("Gamma", volume.gamma.value, 0.1f, 10.0f);
        volume.hue.value = SliderField("Hue", volume.hue.value, 0.0f, 1.0f);
        volume.saturation.value = SliderField("Saturation", volume.saturation.value, 0.0f, 2.0f);

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
