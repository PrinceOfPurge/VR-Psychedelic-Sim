using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.FireTunnel;

/// <summary> Weird: Fire Tunnel demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class FireTunnelDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  private FireTunnelVolume volume;

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

    if (FireTunnel.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out FireTunnelVolume vol) ? vol : null;
    this.enabled = FireTunnel.IsInRenderFeatures() && volume != null;
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

        GUILayout.Label("FIRE TUNNEL DEMO", styleTitle);

        GUILayout.Space(space);

        volume.intensity.value = SliderField("Intensity", volume.intensity.value);

        // Tunnel settings.
        volume.center.value = Vector2Field("Center", volume.center.value);
        volume.speed.value = SliderField("Speed", volume.speed.value, -5.0f, 5.0f);
        volume.tunnelRadius.value = SliderField("Radius", volume.tunnelRadius.value, 0.0f, 10.0f);
        volume.turbulence.value = SliderField("Turbulence", volume.turbulence.value, 0.0f, 2.0f);
        volume.rotation.value = SliderField("Rotation", volume.rotation.value, -5.0f, 5.0f);

        GUILayout.Space(5.0f);

        // Quality settings.
        volume.raymarchSteps.value = SliderField("Steps", volume.raymarchSteps.value, 10, 200);
        volume.noiseScale.value = SliderField("Noise Scale", volume.noiseScale.value, 0.5f, 5.0f);

        GUILayout.Space(5.0f);

        // Color settings.
        volume.fireIntensity.value = SliderField("Fire Intensity", volume.fireIntensity.value, 0.0f, 10.0f);
        volume.colorBlend.value = EnumField("Blend", volume.colorBlend.value);
        volume.fireColor.value = Vector3Field("Fire Color", volume.fireColor.value, 0.0f, 10.0f);

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

  private Vector2 Vector2Field(string label, Vector2 value)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField("  X", value.x, -1.0f, 1.0f);
    value.y = SliderField("  Y", value.y, -1.0f, 1.0f);

    return value;
  }

  private Vector3 Vector3Field(string label, Vector3 value, float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField("  R", value.x, min, max);
    value.y = SliderField("  G", value.y, min, max);
    value.z = SliderField("  B", value.z, min, max);

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
