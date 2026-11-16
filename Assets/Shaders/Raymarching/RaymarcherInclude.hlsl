#ifndef RAYMARCH_INCLUDED
#define RAYMARCH_INCLUDED

#include "SDFLibrary.hlsl"

#define MAX_STEPS 88
#define MAX_DIST 92
#define SURF_DIST 0.03


float4 GetDist(float3 p, float time)
{
    /* // === INFINITE ALIEN LIKE SHAPES START === //
    // 3D FBM Noise at the ray's current position
    float noiseDisplacement1 = fbm(p * 2.0, 3, 0.45);
    noiseDisplacement1 *= 0.08;
    float noiseDisplacement2 = fbm(p * 2.0, 4, 0.5);
    noiseDisplacement2 *= 0.08;

    // FLOOR
    float planeDistance = p.y + 6.0;
    float4 plane = float4(planeDistance, 0.2, 0.2, 0.2); // Floor is dark gray

    // DOMAIN WARPING AND SPACE FOLDING
    // How far apart the copies should be on the X, Y, and Z axes
    float3 spacing = float3(23.7, 27.3, 29.8);
    float3 id = round(p / spacing); // This gives us a unique (x, y, z) integer for whichever grid cell the ray is currently in.
    float3 q = p - spacing * id; // Fold space using the unique ID

    // Breaking the alignment we get from folding our spacing - We use the unique x and z ID of the cell to generate a random height variation.
    q.y += sin(id.x * 67.1 + id.z * 17.6) * 3.0;

    // Mirroring the scene to create a kaleidoscope
    for(int i = 0; i < 3; i++)
    {
        q = abs(q) - 2.0;

        q.xy = mul(Rot(time * 0.3 + id.z*0.5), q.xy);
        q.xz = mul(Rot(time * 0.15 + id.y*0.4), q.xz);
    }
    // ===

    // BOX
    float displacement = sin(p.x * (5.0 + id.x) + time * 3.0) * sin((q.y + id.y) * 5.0) * sin((p.z + id.z) * 5.0 + time * 8.0) * 0.15;
    float3 boxPos = q - float3(0,1,0) + displacement;
    float3 boxSize = float3(1,1,1) + (sin(time*0.77)*0.5+0.5)*0.5;
    float boxDistance = sdBox(boxPos, boxSize) + noiseDisplacement1*0.5;
    float4 box = float4(boxDistance, 0.1, 0.3, 1.0); // Box is bright blue

    // SPHERE
    displacement = sin((p.x + id.x) * 8.0 + time * 4.0) * sin((p.y + id.y) * 3.2 + time * 6.0) * sin((p.z + id.z) * 6.0) * 0.15;
    float3 spherePos = (q - float3(2.5 + sin(time * 2.0),1,0)) + displacement;
    float sphereRadius = 1.0;
    float sphereDistance = (length(spherePos) - sphereRadius) + noiseDisplacement2;
    float4 sphere = float4(sphereDistance, 1.0, 0.1, 0.1); // Sphere is bright red

    // BLENDING THE SCENE
    float4 meltedShapes = sMinColor(box, sphere, 1.0);
    float4 finalScene = minColor(meltedShapes, plane);

    // CARVING OUT 3 CYLINDERS IN THE MIDDLE
    float carvedCylinderXDistance = (sdCylinder(p - float3(0,0,0), float3(-200,0,0), float3(200,0,0), 8)) + noiseDisplacement1;
    float carvedCylinderYDistance = (sdCylinder(p - float3(0,0,0), float3(0,-2,0), float3(0,200,0), 8)) + noiseDisplacement2;
    float carvedCylinderZDistance = (sdCylinder(p - float3(0,0,0), float3(0,0,-200), float3(0,0,200), 8)) + noiseDisplacement1;
    
    // A blend factor of 2.0 ensures the fractal architecture smoothly sags and droops into the clearing
    float carvedCylindersDistance1 = sMin(carvedCylinderXDistance, carvedCylinderYDistance, 2.0);
    float carvedCylindersDistance2 = sMin(carvedCylindersDistance1, carvedCylinderZDistance, 2.0);
    finalScene.x = sMax(finalScene.x, -carvedCylindersDistance2, 2.0);

    finalScene.x *= 0.6; // Safety factor: We're raymarching smaller distances since we're doing a bunch of math up there to the shapes
    return finalScene;
    // === INFINITE ALIEN LIKE SHAPES END === // */


    /* // === KIFS FRACTAL TRY 1 START === //
    // Creating a terrain
    float terrainHeight = ridgeFBM(float3(p.x, 0.0, p.z) * 0.017, 5, 0.35) * 6.3;
    float floorDistance = p.y + (8.0 - terrainHeight);
    float4 floorPlane = float4(floorDistance, 0.15, 0.16, 0.15);

    // DOMAIN WARPING AND SPACE FOLDING
    float3 spacing = float3(19.3, 24.7, 21.1);
    //spacing = float3(100,100,100); // Far away spacing to only focus on one
    float3 id = round(p / spacing); // This gives us a unique (x, y, z) integer for whichever grid cell the ray is currently in.
    float3 q = p - spacing * id; // Fold space using the unique ID

    // Generate three unique random numbers for this specific cell using your hash function
    float h1 = hash(id + float3(11.3, 45.7, 29.1));
    float h2 = hash(id + float3(73.1, 12.5, 84.3));
    float h3 = hash(id + float3(38.9, 91.2, 53.4));

    // Jitter the horizontal position so they aren't lined up in rows
    q.x += (h1 * 2.0 - 1.0) * 3.7;
    q.z += (h2 * 2.0 - 1.0) * 4.3;

    // Breaking the alignment we get from folding our spacing - We use the unique x and z ID of the cell to generate a random height variation.
    q.y += sin(id.x * 67.1 + id.z * 17.6) * 3.0;

    // Randomize the scale factor for each cluster (between 0.6x and 1.4x size)
    float randomScale = lerp(0.8, 1.6, h3);

    // The KIFS Fractal + coloring
    float fractalDistance = sdKIFS(q / randomScale, 6, time) * randomScale;
    
    float distFromCenter = length(q);
    // Drive each RGB channel with independent frequencies based on time and core distance
    float3 vibrantColor = float3(
        sin(time * 1.5 + distFromCenter * 1.2) * 0.5 + 0.5,
        cos(time * 2.2 + distFromCenter * 1.8) * 0.5 + 0.5,
        sin(time * 0.9 + distFromCenter * 2.5) * 0.5 + 0.5
    );
    vibrantColor *= 1.5; // Boost the colors slightly so they pop against the hazy atmosphere
    
    float4 fractal = float4(fractalDistance, vibrantColor);

    // Final scene
    float4 finalScene = sMinColor(fractal, floorPlane, 2.0);
    finalScene.x *= 0.4;
    return finalScene;
    // === KIFS FRACTAL TRY 1 END === // */


    /* // === MEGASTRUCTURE START === //
    float sceneDist = sdMegaStructure(p, time);

    float3 concreteColor = float3(0.3, 0.3, 0.32);
    float4 scene = float4(sceneDist, concreteColor);

    scene.x *= 0.5;
    return scene;
    // === MEGASTRUCTURE END === // */

    
    // === MENGER SPONGE START === //
    float4 sponge = sdMengerSponge(p-12.0, time);

    sponge.x *= 0.8f;
    return sponge;
    // === MENGER SPONGE END === //

    
    /* // === MANDELBULB START === //
    // The mandelbulb lives inside a [-2,2] radius
    
    float modelScale = 35.0;
    
    float4 bulb = sdMandelbulb(p / modelScale, time); // Scale the coordinate down so it fits the Mandelbulb's tiny native domain
    bulb.x *= modelScale;
    bulb.x *= 0.5f;
    
    return bulb;
    // === MANDELBULB END === // */
}

float Raymarch(float3 ro, float3 rd, float time)
{
    float dO = 0; // Distance from origin
    
    for (int i = 0; i < MAX_STEPS; i++)
    {
        float3 p = ro + dO * rd; // Our position is our origin, plus our distance from origin in ray direction
        float dS = GetDist(p, time).x; // We only need the distance here to do the raymarching (yzw values are rgb)
        dO += dS; // Stepping forward in the safe distance we got

        if (dS < (SURF_DIST) || dO > MAX_DIST) break;
    }
    return dO;
}


// === LIGHTING FUNCTIONS START === //
float3 GetNormal(float3 p, float time)
{
    float2 offset = float2(0.01, 0);
    float3 n = GetDist(p, time).x - float3(GetDist(p-offset.xyy, time).x, GetDist(p-offset.yxy, time).x, GetDist(p-offset.yyx, time).x);
    return normalize(n);
}

float GetLight(float3 p, float time)
{
    #ifndef SHADERGRAPH_PREVIEW
    Light mainLight = GetMainLight();
    float3 lightDir = mainLight.direction;  
    #else
    float3 lightDir = float3(0.5, 0.8, 0.2); // Mock light for the preview window
    #endif

    float3 lightPos = float3(0,5,6); 
    float3 l = normalize(lightDir);
    float3 n = GetNormal(p, time);
    float diff = clamp(dot(n,l), 0.0, 1.0);

    // Shadows
    float d = Raymarch(p+(n*SURF_DIST*2.0), l, time);
    if (d < length(lightPos-p)) diff *= 0.2;
    
    return diff;
}

float GetAO(float3 p, float3 n, float time)
{
    // Ambient Occlusion: We shoot out a few tiny, invisible "probes" straight out from the surface using the normal
    // If those probes don't hit anything, the surface is exposed to the light and bright. If we hit something, it means we are in a
    // tight fold, and we darken the pixel
    
    float ao = 0.0;
    float stepSize = 0.15; // How far each probe steps away from the surface
    float weight = 1.0; // How much the occlusion matters

    // Step out from the surface 5 times
    for (int i = 0; i <= 4; i++)
    {
        float dist = stepSize * i;

        // Check the actual distance to the nearest geometry at this point
        float d = GetDist(p + n * dist, time).x;

        // If the math says geometry is closer than our probe's distance, we are in a tight corner
        ao += max(0.0, (dist - d)) * weight;

        // Occlusion matters less the further away our probe gets
        weight *= 0.5;
    }

    // Subtract the trapped shadow value from 1.0 (fully lit) and clamp it
    return clamp(1.0 - (ao * 3.0), 0.0, 1.0);
}
// === LIGHTING FUNCTIONS END === //


// The wrapper function specifically for Shader Graph
void RaymarchNode_float(float3 ro, float3 hitPos, float time, out float3 OutColor, out float OutAlpha)
{
    float3 rd = normalize(hitPos - ro); // Ray direction is where we hit minus where our camera is, normalized
    float d = Raymarch(ro, rd, time);


    // ==========================================
    // SCENE (DISTANCE FOG AND COLOURING)
    // ==========================================
    // Dynamic sky gradient based on the ray's vertical tilt
    float skyAlpha = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0); // rd.y goes between -1 to 1, so we just bring the value up
    float3 skyColor = float3(.01,.01,.01);
    float3 horizonColor = float3(.001,.001,.001);
    float3 fogCol = lerp(horizonColor, skyColor, skyAlpha);
    
    float fogStart = 80.0;
    float fogEnd = 120.0;
    
    if (d >= MAX_DIST) 
    {
        // If we didn't hit any geometry, draw the pure fog color to create an infinite horizon
        OutColor.rgb = fogCol;
        OutAlpha = 1.0;
    } 
    else 
    {
        // Calculate lighting
        float3 p = ro + rd * d; // Our point in 3d space
        float diffuse = GetLight(p, time);
        
        #ifndef SHADERGRAPH_PREVIEW
            Light mainLight = GetMainLight();
            float3 lightCol = mainLight.color;
        #else
            float3 lightCol = float3(1, 1, 1);
        #endif

        // Link shadow color with horizon color - This ensures unlit areas receive realistic ambient light from the environment
        float3 shadowCol = horizonColor * 0.25;
        float smoothedDiffuse = smoothstep(0.0, 1.0, diffuse);

        // Get the normal and calculate AO
        float3 n = GetNormal(p, time);
        float ao = GetAO(p, n, time);
        
        // Get the color of the object we just hit: We call GetDist at our exact surface point 'p' to get the float4 data
        float3 objectColor = GetDist(p, time).yzw;
        
        // Apply ao by multiplying it against the lighting gradient so we get darker shadows in tighter spots
        float3 lightingGradient = lerp(shadowCol, lightCol, smoothedDiffuse) * ao;
        
        // HDR Emission: We multiply the orbit trap color by the fresnel rim factor and boost it into high HDR values
        // Multiply the glow by pow(ao, 3.0). This acts as a shadow mask, instantly killing the glow (and the noise) in tight crevices.
        float3 baseOpaqueCol = (objectColor*0.2) * lightingGradient; // Keep the base fractal dark
        float fresnel = pow(1.0 - max(0.0, dot(n, -rd)), 12.0); // Detect silhouettes of the fractal bulbs
        float3 emissiveGlow = objectColor * (fresnel * 4.0 + 0.4) * pow(ao, 4);
        float3 finalCol = baseOpaqueCol + emissiveGlow; // We add the glowing emission on top of the dark structure

        // Apply Depth Fog
        float fogAmount = smoothstep(fogStart, fogEnd, d); // Returns a 0-1 value based on how far 'd' is between fogStart and fogEnd
        float3 opaqueSceneColor = lerp(finalCol, fogCol, fogAmount);
        
        // Output scene
        OutColor.rgb = opaqueSceneColor;
        OutAlpha = 1.0;
    }
}

#endif