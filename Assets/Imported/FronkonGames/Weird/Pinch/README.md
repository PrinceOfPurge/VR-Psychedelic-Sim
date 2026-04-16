# Weird: Pinch

A raymarching-based post-processing effect that creates a dynamic pinch distortion on the screen. Click and drag to create a conical extrusion effect that follows your pointer, with configurable start and end areas, roundness, and Lambert lighting.

## 01. REQUISITES

To ensure optimal performance and compatibility, your project must meet the following requirements:

*   **Unity:** 6000.0.58f1 or higher.
*   **Universal RP:** 17.0.3 or higher.

## 02. INSTALLATION GUIDE

### Step 1: Add Renderer Feature

The effect must be registered in your project's URP configuration:

1. Locate your **Universal Renderer Data** asset.
2. Click **Add Renderer Feature** and select **Fronkon Games > Weird > Pinch**.

### Step 2: Configure the Volume

To apply the effect to your scene:

1. Create a **Volume** component (Global or Local).
2. In the Volume component, create or assign a **Volume Profile**.
3. Click **Add Override** and select **Fronkon Games > Weird > Pinch**.
4. Enable the '**Intensity**' parameter (and any others you wish to modify).

## 03. CONTACT & SUPPORT

*   **Email:** [fronkongames@gmail.com](mailto:fronkongames@gmail.com)
*   **Documentation:** [Online Help](https://fronkongames.github.io/store/weird/)

**NOTICE:** This asset is licensed for use in your projects but **cannot be hosted in public repositories**.
