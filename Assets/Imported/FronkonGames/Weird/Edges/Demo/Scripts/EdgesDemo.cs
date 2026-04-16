using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Edges;

/// <summary> Weird: Edges demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class EdgesDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private EdgesVolume volume;

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

    if (Edges.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out EdgesVolume vol) ? vol : null;
    this.enabled = Edges.IsInRenderFeatures() && volume != null;
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

        GUILayout.Label("EDGES DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value);

        // Edges settings.
        volume.edges.value = SliderField("Edges", volume.edges.value, 0.0f, 1.0f);
        volume.edgeWidth.value = SliderField("  Width", volume.edgeWidth.value, 1, 10);
        volume.edgeThreshold.value = SliderField("  Threshold", volume.edgeThreshold.value, 0.01f, 1.0f);
        volume.edgeDetectionMethod.value = EnumField("  Method", volume.edgeDetectionMethod.value);
        volume.edgeColorMode.value = EnumField("  Color Mode", volume.edgeColorMode.value);
        if (volume.edgeColorMode.value == EdgeColorMode.Linear)
        {
          volume.edgesColor.value = ColorField("  Color", volume.edgesColor.value);
        }
        else if (volume.edgeColorMode.value == EdgeColorMode.DepthBased)
        {
          volume.depthEdgeNearColor.value = ColorField("  Near Color", volume.depthEdgeNearColor.value);
          volume.depthEdgeFarColor.value = ColorField("  Far Color", volume.depthEdgeFarColor.value);
          volume.depthEdgeNear.value = SliderField("  Near", volume.depthEdgeNear.value, 0.0f, 1.0f);
          volume.depthEdgeFar.value = SliderField("  Far", volume.depthEdgeFar.value, 0.0f, 1.0f);
        }
        volume.edgesBlend.value = EnumField("  Blend", volume.edgesBlend.value);

        GUILayout.Space(5.0f);

        // Background settings.
        volume.backgroundColor.value = ColorField("Background", volume.backgroundColor.value);
        volume.backgroundBlend.value = EnumField("  Blend", volume.backgroundBlend.value);
        volume.ignoreSkybox.value = ToggleField("Ignore Skybox", volume.ignoreSkybox.value);

        GUILayout.Space(5.0f);

        // Color adjustments.
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
