using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Spiral;

/// <summary> Weird: Spiral demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class SpiralDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private SpiralVolume volume;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void ResetEffect()
  {
    volume.Reset();
    volume.intensity.value = 1.0f;
    volume.intensity.overrideState = true;
  }

  private void Awake()
  {
    styleTitle = styleLabel = styleButton = null;

    if (Spiral.IsInAnyRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out SpiralVolume vol) ? vol : null;
    this.enabled = Spiral.IsInAnyRenderFeatures() && volume != null;
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

        GUILayout.Label("SPIRAL DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value, 0.0f, 1.0f);
        volume.intensity.overrideState = volume.intensity.value > 0.0f;

        GUILayout.Space(space);

        // Wrap
        volume.wrap.value = SliderField("Wrap", volume.wrap.value, 0.0f, 1.0f);
        volume.wrap.overrideState = true;
        volume.shape.value = EnumField("  Shape", volume.shape.value);
        volume.shape.overrideState = true;
        volume.center.value = Vector2Field("  Center", volume.center.value, "X", "Y", -0.5f, 0.5f);
        volume.center.overrideState = true;
        volume.spiralAmount.value = SliderField("  Amount", volume.spiralAmount.value, 0.0f, 1.0f);
        volume.spiralAmount.overrideState = true;
        volume.rotation.value = SliderField("  Rotation", volume.rotation.value, 0.0f, 360.0f);
        volume.rotation.overrideState = true;
        volume.rotationSpeed.value = SliderField("  Rotation speed", volume.rotationSpeed.value, -2.0f, 2.0f);
        volume.rotationSpeed.overrideState = true;
        volume.outerRing.value = SliderField("  Outer ring", volume.outerRing.value, 0.0f, 1.0f);
        volume.outerRing.overrideState = true;
        volume.zoomSpeed.value = SliderField("  Zoom speed", volume.zoomSpeed.value, -2.0f, 2.0f);
        volume.zoomSpeed.overrideState = true;
        volume.frequency.value = SliderField("  Frequency", volume.frequency.value, 0.1f, 5.0f);
        volume.frequency.overrideState = true;
        volume.edgeMode.value = EnumField("  Edge mode", volume.edgeMode.value);
        volume.edgeMode.overrideState = true;

        GUILayout.Space(space);

        // Outer tint
        volume.outerTintIntensity.value = SliderField("Outer tint", volume.outerTintIntensity.value, 0.0f, 1.0f);
        volume.outerTintIntensity.overrideState = true;
        volume.outerTintColor.value = ColorField("  Color", volume.outerTintColor.value);
        volume.outerTintColor.overrideState = true;
        volume.outerTintSoftness.value = SliderField("  Softness", volume.outerTintSoftness.value, 0.1f, 5.0f);
        volume.outerTintSoftness.overrideState = true;
        volume.outerTintColorBlend.value = EnumField("  Blend", volume.outerTintColorBlend.value);
        volume.outerTintColorBlend.overrideState = true;

        GUILayout.Space(space);

        // Shadow
        volume.shadowIntensity.value = SliderField("Shadow", volume.shadowIntensity.value, 0.0f, 1.0f);
        volume.shadowIntensity.overrideState = true;
        volume.shadowColor.value = ColorField("  Color", volume.shadowColor.value);
        volume.shadowColor.overrideState = true;
        volume.shadowSoftness.value = SliderField("  Softness", volume.shadowSoftness.value, 0.1f, 5.0f);
        volume.shadowSoftness.overrideState = true;
        volume.shadowOffset.value = SliderField("  Offset", volume.shadowOffset.value, -1.0f, 1.0f);
        volume.shadowOffset.overrideState = true;
        volume.shadowColorBlend.value = EnumField("  Blend", volume.shadowColorBlend.value);
        volume.shadowColorBlend.overrideState = true;

        GUILayout.Space(space);

        // Line
        volume.lineWidth.value = SliderField("Line", volume.lineWidth.value, 0.0f, 1.0f);
        volume.lineWidth.overrideState = true;
        volume.lineColor.value = ColorField("  Color", volume.lineColor.value);
        volume.lineColor.overrideState = true;
        volume.lineSoftness.value = SliderField("  Softness", volume.lineSoftness.value, 0.01f, 1.0f);
        volume.lineSoftness.overrideState = true;
        volume.lineColorBlend.value = EnumField("  Blend", volume.lineColorBlend.value);
        volume.lineColorBlend.overrideState = true;
        volume.lineCount.value = SliderField("  Count", volume.lineCount.value, 1, 10);
        volume.lineCount.overrideState = true;

        GUILayout.Space(space);

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
