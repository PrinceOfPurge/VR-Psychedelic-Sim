// === Structure with color functions ===
struct ColorUtils
{
    float3 RGBtoHCV(float3 RGB)
    {
        float Epsilon = 1e-10;
        float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
        float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
        float C = Q.x - min(Q.w, Q.y);
        float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
        return float3(H, C, Q.x);
    }

    float3 RGBtoHSL(float3 RGB)
    {
        float Epsilon = 1e-10;
        float3 HCV = RGBtoHCV(RGB);
        float L = HCV.z - HCV.y * 0.5;
        float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
        return float3(HCV.x, S, L);
    }

    float3 HUEtoRGB(in float H)
    {
        float R = abs(H * 6 - 3) - 1;
        float G = 2 - abs(H * 6 - 2);
        float B = 2 - abs(H * 6 - 4);
        return saturate(float3(R,G,B));
    }

    float3 HSLtoRGB(float3 HSL)
    {
        float3 RGB = HUEtoRGB(HSL.x);
        float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
        return (RGB - 0.5) * C + HSL.z;
    }
};
ColorUtils colorUtils;


void TrippyEffect_float(float2 UV, UnityTexture2D _CameraOpaqueTexture, float Strength, float Time, out float3 Out)
{
    const float zoomFactor = 1.0;
    const float swirlIntensity = 0.3;
    const float swirlSpeed = 0.1;
    const float swirlFrequency = 6.0;
    const float swirlTimeShift = 0.3;
    const float numSegments = 6.0;
    const float waveFrequencyX = 2.0;
    const float waveFrequencyY = 3.0;
    const float waveSpeedX = 0.2;
    const float waveSpeedY = 0.15;
    const float waveStrengthX = 0.05;
    const float waveStrengthY = 0.03;
    const float hueShiftSpeed = 0.2;
    const float hueShiftIntensity = 0.6;
    const float hueShiftFrequency = 0.3;

    float2 distortedUVs = UV;
    distortedUVs += 0.002 * sin(Time * 0.5 + UV * 10.0);

    float2 centeredUV = UV - 0.5;
    float radius = length(centeredUV);
    float angle = atan2(centeredUV.y, centeredUV.x);

    float segmentSize = 6.283185 / numSegments;
    angle = fmod(angle, segmentSize) - segmentSize / 2.0;
    angle = abs(angle);

    float dynamicSwirl = sin(Time * swirlSpeed) * swirlIntensity;
    angle += dynamicSwirl * sin(radius * swirlFrequency - Time * swirlTimeShift);

    distortedUVs.x = 0.5 + radius * cos(angle);
    distortedUVs.y = 0.5 + radius * sin(angle);

    distortedUVs.x += sin(UV.y * waveFrequencyY + Time * waveSpeedY) * waveStrengthY;
    distortedUVs.y += cos(UV.x * waveFrequencyX + Time * waveSpeedX) * waveStrengthX;

    distortedUVs = (distortedUVs - 0.5) * zoomFactor + 0.5;
    float2 finalUVs = lerp(UV, distortedUVs, Strength);

    float4 sceneColor = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture.tex, _CameraOpaqueTexture.samplerstate, finalUVs, 0);

    float3 HSL = colorUtils.RGBtoHSL(sceneColor.rgb);
    HSL.x += sin(Time * hueShiftSpeed + distortedUVs.x * hueShiftFrequency) * hueShiftIntensity;
    HSL.x = frac(HSL.x);
    float3 trippyColor = colorUtils.HSLtoRGB(HSL);

    Out = lerp(sceneColor.rgb, trippyColor.rgb, Strength);
}


void TrippyUV_float(float2 UV, float Strength, float Time, out float2 TrippyUV)
{
    const float zoomFactor = 1.0;
    const float swirlIntensity = 0.3;
    const float swirlSpeed = 0.1;
    const float swirlFrequency = 6.0;
    const float swirlTimeShift = 0.3;
    const float numSegments = 6.0;
    const float waveFrequencyX = 2.0;
    const float waveFrequencyY = 3.0;
    const float waveSpeedX = 0.2;
    const float waveSpeedY = 0.15;
    const float waveStrengthX = 0.05;
    const float waveStrengthY = 0.03;

    // Base UV warp
    float2 distortedUVs = UV;
    distortedUVs += 0.002 * sin(Time * 0.5 + UV * 10.0);

    // Polar conversion
    float2 centeredUV = UV - 0.5;
    float radius = length(centeredUV);
    float angle = atan2(centeredUV.y, centeredUV.x);

    // Kaleidoscope
    float segmentSize = 6.283185 / numSegments;
    angle = fmod(angle, segmentSize) - segmentSize / 2.0;
    angle = abs(angle);

    // Swirl
    float dynamicSwirl = sin(Time * swirlSpeed) * swirlIntensity;
    angle += dynamicSwirl * sin(radius * swirlFrequency - Time * swirlTimeShift);

    // Back to Cartesian
    distortedUVs.x = 0.5 + radius * cos(angle);
    distortedUVs.y = 0.5 + radius * sin(angle);

    // Organic waves
    distortedUVs.x += sin(UV.y * waveFrequencyY + Time * waveSpeedY) * waveStrengthY;
    distortedUVs.y += cos(UV.x * waveFrequencyX + Time * waveSpeedX) * waveStrengthX;

    // Apply zoom
    distortedUVs = (distortedUVs - 0.5) * zoomFactor + 0.5;

    // Blend with original UVs
    TrippyUV = lerp(UV, distortedUVs, Strength);
}

void TrippyColor_float(float3 SceneColor, float2 TrippyUV, float Strength, float Time, out float3 OutColor)
{
    const float hueShiftSpeed = 0.2;
    const float hueShiftIntensity = 0.6;
    const float hueShiftFrequency = 0.3;

    float3 HSL = colorUtils.RGBtoHSL(SceneColor);
    HSL.x += sin(Time * hueShiftSpeed + TrippyUV.x * hueShiftFrequency) * hueShiftIntensity;
    HSL.x = frac(HSL.x);
    float3 trippyColor = colorUtils.HSLtoRGB(HSL);

    OutColor = lerp(SceneColor, trippyColor, Strength);
}
