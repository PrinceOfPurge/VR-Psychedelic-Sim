using System;
using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Weird.Pinch;

/// <summary> Weird: Pinch demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class PinchDemo : MonoBehaviour
{
  [Header("This code is only for the demo, not for production environments.")]

  [Space(20.0f), SerializeField]
  private VolumeProfile volumeProfile;

  [SerializeField, Range(0.1f, 10.0f)]
  private float tweenDuration = 1.0f;

  [SerializeField, Range(0.01f, 1.0f)]
  private float easeInPortion = 0.2f;

  [SerializeField]
  private AudioClip audioBoing;

  [SerializeField]
  private AudioClip audioStretch;

  private PinchVolume volume;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private bool isDragging = false;
  private bool isTweening = false;
  private Vector2 startPoint;
  private Vector2 endPoint;
  private Vector2 tweenStartPoint;
  private float tweenTime = 0.0f;
  private AudioSource audioSource;
  private bool hasPlayedBoing = false;

  private void ResetEffect()
  {
    volume.Reset();
    volume.intensity.value = 1.0f;
    volume.end.value = Vector2.zero;
    
    isDragging = false;
    isTweening = false;
    startPoint = volume.start.value;
    endPoint = volume.end.value + volume.start.value;
    hasPlayedBoing = false;
    
    if (audioSource != null && audioSource.isPlaying)
      audioSource.Stop();
  }

  private void Awake()
  {
    styleTitle = styleLabel = styleButton = null;

    if (Pinch.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    volume = volumeProfile != null && volumeProfile.TryGet(out PinchVolume vol) ? vol : null;
    this.enabled = Pinch.IsInRenderFeatures() && volume != null;
  }

  private void Start()
  {
    ResetEffect();
    
    startPoint = volume.start.value;
    endPoint = volume.end.value + volume.start.value;
    
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;
  }

  private void Update()
  {
    if (isTweening)
    {
      UpdateTween();
      return;
    }

    // Don't process input if mouse is over GUI (GUI panel is 450px wide)
    if (Input.mousePosition.x < 450.0f)
      return;

    if (Input.GetMouseButtonDown(0) == true)
    {
      Vector2 mousePos = Input.mousePosition;
      startPoint = new Vector2(mousePos.x / Screen.width, (mousePos.y / Screen.height));
      endPoint = startPoint;
      
      volume.start.value = startPoint;
      volume.end.value = Vector2.zero;
      
      isDragging = true;
      
      if (audioStretch != null && audioSource != null)
      {
        audioSource.clip = audioStretch;
        audioSource.loop = true;
        audioSource.Play();
      }
    }
    else if (Input.GetMouseButton(0) && isDragging)
    {
      Vector2 mousePos = Input.mousePosition;
      Vector2 mouseNormalized = new Vector2(mousePos.x / Screen.width, (mousePos.y / Screen.height));
      endPoint = mouseNormalized;
      
      volume.end.value = endPoint - startPoint;
      
      if (audioStretch != null && audioSource != null && !audioSource.isPlaying)
      {
        audioSource.clip = audioStretch;
        audioSource.loop = true;
        audioSource.Play();
      }
    }
    else if (Input.GetMouseButtonUp(0) && isDragging)
    {
      isDragging = false;
      
      if (audioSource != null && audioSource.isPlaying)
        audioSource.Stop();
      
      isTweening = true;
      tweenStartPoint = endPoint;
      tweenTime = 0.0f;
      hasPlayedBoing = false;
    }
  }

  private void UpdateTween()
  {
    if (!hasPlayedBoing && audioBoing != null && audioSource != null)
    {
      audioSource.clip = audioBoing;
      audioSource.loop = false;
      audioSource.Play();
      hasPlayedBoing = true;
    }
    
    tweenTime += Time.deltaTime;
    float t = Mathf.Clamp01(tweenTime / tweenDuration);
    
    float easedT = EaseInOutElastic(t);
    
    endPoint = Vector2.Lerp(tweenStartPoint, startPoint, easedT);
    volume.end.value = endPoint - startPoint;
    
    if (t >= 1.0f)
    {
      isTweening = false;
      endPoint = startPoint;
      volume.end.value = Vector2.zero;
      hasPlayedBoing = false;
    }
  }

  private float EaseInOutElastic(float t)
  {
    if (t < easeInPortion)
      return EaseInQuad(t / easeInPortion) * easeInPortion;

    float elasticT = (t - easeInPortion) / (1.0f - easeInPortion);
    return easeInPortion + EaseOutElastic(elasticT) * (1.0f - easeInPortion);
  }

  private float EaseInQuad(float t) => t * t;

  private float EaseOutElastic(float t)
  {
    if (t <= 0.0f)
      return 0.0f;
    if (t >= 1.0f)
      return 1.0f;

    const float p = 0.3f;      // Period (controls oscillation frequency)
    const float s = p / 40.0f; // Amplitude scaling
    
    return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (2.0f * Mathf.PI) / p) + 1.0f;
  }

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

        GUILayout.Label("CLICK AND DRAG TO MAKE A PINCH EFFECT", styleTitle);

        GUILayout.Space(space);

        volume.startRadius.value = SliderField("Start Radius", volume.startRadius.value, 0.0f, 1.0f);
        volume.endRadius.value = SliderField("End Radius", volume.endRadius.value, 0.0f, 1.0f);
        volume.roundness.value = SliderField("Roundness", volume.roundness.value, 0.0f, 1.0f);
        volume.lightIntensity.value = SliderField("Light Intensity", volume.lightIntensity.value, 0.0f, 1.0f);
        volume.lightDirection.value = Vector3Field("Light Direction", volume.lightDirection.value, "X", "Y", "Z", 0.0f, 1.0f);

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
