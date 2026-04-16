using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Bubbles;

/// <summary> Weird: Bubbles demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class BubblesDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private BubblesVolume volume;
  
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

    if (Bubbles.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    volume = volumeProfile != null && volumeProfile.TryGet(out BubblesVolume vol) ? vol : null;
    this.enabled = Bubbles.IsInRenderFeatures() && volume != null;
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

        GUILayout.Label("BUBBLES DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value);

        // Bubbles settings.
        volume.bubbleColor.value = ColorField("Bubble Color", volume.bubbleColor.value);
        volume.bubbleColorBlend.value = EnumField("  Color Blend", volume.bubbleColorBlend.value);
        volume.bubbleRoundness.value = SliderField("  Roundness", volume.bubbleRoundness.value, 0.0f, 1.0f);
        volume.bubbleSize.value = SliderField("  Size", volume.bubbleSize.value, 10.0f, 100.0f);
        volume.bubbleBevel.value = SliderField("  Bevel", volume.bubbleBevel.value, 0.05f, 1.0f);
        volume.bubbleSpacing.value = SliderField("  Spacing", volume.bubbleSpacing.value, 0.0f, 1.0f);

        GUILayout.Space(5.0f);

        // Lighting settings.
        volume.lightSpecular.value = SliderField("Lighting", volume.lightSpecular.value, 0.0f, 2.0f);
        volume.lightColor.value = ColorField("  Color", volume.lightColor.value);
        volume.lightSpecularPower.value = SliderField("  Power", volume.lightSpecularPower.value, 1.0f, 200.0f);
        volume.lightAngle.value = SliderField("  Angle", volume.lightAngle.value, 0.0f, 360.0f);
        volume.lightElevation.value = SliderField("  Elevation", volume.lightElevation.value, 0.0f, 90.0f);

        GUILayout.Space(5.0f);

        // Background settings.
        volume.backgroundColor.value = ColorField("Background", volume.backgroundColor.value);
        volume.backgroundBlend.value = EnumField("  Blend", volume.backgroundBlend.value);
        volume.backgroundBlur.value = SliderField("  Blur", volume.backgroundBlur.value, 0.0f, 20.0f);
        volume.backgroundExposure.value = SliderField("  Exposure", volume.backgroundExposure.value, -2.0f, 2.0f);

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
