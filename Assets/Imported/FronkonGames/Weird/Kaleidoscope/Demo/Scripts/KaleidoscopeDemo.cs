using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Kaleidoscope;

/// <summary> Weird: Kaleidoscope demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class KaleidoscopeDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private KaleidoscopeVolume volume;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void ResetEffect()
  {
    volume.Reset();
    volume.intensity.value = 1.0f;
  }

  private void Awake()
  {
    styleTitle = styleLabel = styleButton = null;
/*
if (Kaleidoscope.IsInRenderFeatures() == false)
{
  Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
  
#if UNITY_EDITOR
  if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
    UnityEditor.EditorApplication.isPlaying = false;
#endif
}
*/
volume = volumeProfile != null && volumeProfile.TryGet(out KaleidoscopeVolume vol) ? vol : null;
this.enabled = volume != null;
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

    GUILayout.Label("KALEIDOSCOPE DEMO", styleTitle);

    GUILayout.Space(space);

    volume.intensity.value = SliderField("Intensity", volume.intensity.value, 0.0f, 1.0f);

    GUILayout.Space(space);

    volume.center.value = Vector2Field("Center", volume.center.value);
    volume.iterationCount.value = SliderField("  Iterations", volume.iterationCount.value, 1, 10);
    volume.speed.value = SliderField("  Speed", volume.speed.value, -10.0f, 10.0f);
    volume.scale.value = SliderField("  Scale", volume.scale.value, 0.1f, 10.0f);

    GUILayout.Space(space);

    /// Offset UV
    volume.offsetIntensity.value = SliderField("Offset UV", volume.offsetIntensity.value, 0.0f, 1.0f);
    volume.offsetRedScale.value = Vector2Field("  Red", volume.offsetRedScale.value);
    volume.offsetGreenScale.value = Vector2Field("  Green", volume.offsetGreenScale.value);
    volume.offsetBlueScale.value = Vector2Field("  Blue", volume.offsetBlueScale.value);
    volume.offsetScale.value = SliderField("  Scale", volume.offsetScale.value, 0.0f, 10.0f);

    GUILayout.Space(space);

    /// Color
    volume.colorIntensity.value = SliderField("Color", volume.colorIntensity.value, 0.0f, 1.0f);
    volume.colorPalette.value = EnumField("  Palette", volume.colorPalette.value);
    volume.blend.value = EnumField("  Blend", volume.blend.value);
    volume.brightness.value = SliderField("  Brightness", volume.brightness.value, -1.0f, 1.0f);
    volume.contrast.value = SliderField("  Contrast", volume.contrast.value, 0.0f, 10.0f);
    volume.gamma.value = SliderField("  Gamma", volume.gamma.value, 0.1f, 10.0f);
    volume.hue.value = SliderField("  Hue", volume.hue.value, 0.0f, 1.0f);
    volume.saturation.value = SliderField("  Saturation", volume.saturation.value, 0.0f, 2.0f);

    GUILayout.Space(space);

    /// Segment
    volume.segmentIntensity.value = SliderField("Segment", volume.segmentIntensity.value, 0.0f, 1.0f);
    volume.segmentBlend.value = EnumField("  Blend", volume.segmentBlend.value);
    volume.segmentColor.value = ColorField("  Color", volume.segmentColor.value);
    volume.segmentWidth.value = SliderField("  Width", volume.segmentWidth.value, 0.0f, 1.0f);

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
