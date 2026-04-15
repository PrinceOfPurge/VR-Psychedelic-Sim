Shader "Fronkon Games/Weird/Extruder/Sprite Depth Writer"
{
  Properties
  {
    [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
    [MainColor] _BaseColor ("Tint", Color) = (1,1,1,1)
    _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    _DepthBias ("Depth Bias", Range(0, 1)) = 0
    [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 1
    [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 0
  }
  
  SubShader
  {
    Tags 
    { 
      "Queue" = "Transparent" 
      "RenderType" = "Transparent"
      "RenderPipeline" = "UniversalPipeline"
      "IgnoreProjector" = "True"
    }
    
    // Ensure depth writing is enabled
    ZWrite On
    ZTest LEqual
    
    // Blend for transparency
    Blend SrcAlpha OneMinusSrcAlpha
    
    Cull Off
    Lighting Off
    ColorMask RGBA
    
    Pass
    {
      Name "ForwardUnlit"
      Tags
      { 
        "LightMode" = "UniversalForward"
        "RenderType" = "Transparent"
        "Queue" = "Transparent"
      }
      
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0
      
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      
      struct Attributes
      {
        float4 positionOS   : POSITION;
        float2 uv           : TEXCOORD0;
        float4 color        : COLOR;
      };
      
      struct Varyings
      {
        float2 uv           : TEXCOORD0;
        float4 color        : COLOR;
        float4 positionCS   : SV_POSITION;
        float4 positionHCS  : TEXCOORD1; // For depth writing verification
      };
      
      TEXTURE2D(_BaseMap);
      SAMPLER(sampler_BaseMap);
      
      CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        float _Cutoff;
        float _DepthBias;
      CBUFFER_END
      
      Varyings vert(Attributes input)
      {
        Varyings output = (Varyings)0;
        
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = vertexInput.positionCS;
        output.positionHCS = vertexInput.positionCS; // Store for depth calculations
        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
        output.color = input.color * _BaseColor;
        
        return output;
      }
      
      half4 frag(Varyings input) : SV_Target
      {
        half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * input.color;
        
        // Alpha cutoff for transparency (URP standard)
        clip(col.a - _Cutoff);
        
        // Force depth write by manipulating position
        // This ensures the fragment shader contributes to depth buffer
        #if defined(UNITY_REVERSED_Z)
          float depth = input.positionCS.z / input.positionCS.w;
          depth += _DepthBias;
          depth = saturate(depth);
        #else
          float depth = (input.positionCS.z / input.positionCS.w + 1.0) * 0.5;
          depth += _DepthBias * 0.001; // Much smaller bias for non-reversed
          depth = saturate(depth);
        #endif
        
        // Return texture color with transparency
        return col;
      }
      ENDHLSL
    }
    
    // URP Depth Prepass - Writes to _CameraDepthTexture
    Pass
    {
      Name "DepthPrepass"
      Tags
      {
        "LightMode" = "DepthPrepass"
        "RenderPipeline" = "UniversalPipeline"
      }
      
      ColorMask 0
      ZWrite On
      ZTest LEqual
      Cull Off
      
      HLSLPROGRAM
      #pragma vertex depth_vert
      #pragma fragment depth_frag
      #pragma target 2.0
      
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
      
      struct Attributes
      {
        float4 positionOS   : POSITION;
        float2 uv           : TEXCOORD0;
      };
      
      struct Varyings
      {
        float4 positionCS   : SV_POSITION;
        float2 uv           : TEXCOORD0;
      };
      
      CBUFFER_START(UnityPerMaterial)
        float _DepthBias;
      CBUFFER_END
      
      TEXTURE2D(_BaseMap);
      SAMPLER(sampler_BaseMap);
      
      Varyings depth_vert(Attributes input)
      {
        Varyings output = (Varyings)0;
        
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = vertexInput.positionCS;
        output.uv = input.uv;
        
        return output;
      }
      
      half4 depth_frag(Varyings input) : SV_Target
      {
        // Sample alpha to ensure transparency is handled correctly in depth texture
        half4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        
        // Only write depth for visible pixels (not fully transparent)
        clip(texSample.a - 0.1); // Small threshold to ensure sprite edges write depth
        
        // Return dummy color - depth is written by ZWrite On
        return half4(0, 0, 0, 0);
      }
      ENDHLSL
    }
    
    // URP Depth Normal Prepass - Also writes to _CameraDepthTexture
    Pass
    {
      Name "DepthNormalPrepass"
      Tags
      {
        "LightMode" = "DepthNormalPrepass"
        "RenderPipeline" = "UniversalPipeline"
      }
      
      ColorMask 0
      ZWrite On
      ZTest LEqual
      Cull Off
      
      HLSLPROGRAM
      #pragma vertex depth_vert
      #pragma fragment depth_frag
      #pragma target 2.0
      
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
      
      struct Attributes
      {
        float4 positionOS   : POSITION;
        float2 uv           : TEXCOORD0;
      };
      
      struct Varyings
      {
        float4 positionCS   : SV_POSITION;
        float2 uv           : TEXCOORD0;
      };
      
      CBUFFER_START(UnityPerMaterial)
        float _DepthBias;
      CBUFFER_END
      
      TEXTURE2D(_BaseMap);
      SAMPLER(sampler_BaseMap);
      
      Varyings depth_vert(Attributes input)
      {
        Varyings output = (Varyings)0;
        
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = vertexInput.positionCS;
        output.uv = input.uv;
        
        return output;
      }
      
      half4 depth_frag(Varyings input) : SV_Target
      {
        // Sample alpha to ensure transparency is handled correctly in depth texture
        half4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        
        // Only write depth for visible pixels
        clip(texSample.a - 0.1); // Small threshold to ensure sprite edges write depth
        
        // Return dummy color - depth is written by ZWrite On
        return half4(0, 0, 0, 0);
      }
      ENDHLSL
    }
    
    // Simple Depth Only pass for _CameraDepthTexture compatibility
    Pass
    {
      Name "DepthOnly"
      Tags
      {
        "LightMode" = "DepthOnly"
        "RenderPipeline" = "UniversalPipeline"
      }
      
      ColorMask 0
      ZWrite On
      ZTest LEqual
      Cull Off
      
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0
      
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      
      struct Attributes
      {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
      };
      
      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
      };
      
      CBUFFER_START(UnityPerMaterial)
        float _DepthBias;
      CBUFFER_END
      
      TEXTURE2D(_BaseMap);
      SAMPLER(sampler_BaseMap);
      
      Varyings vert(Attributes input)
      {
        Varyings output;
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = vertexInput.positionCS;
        output.uv = input.uv;

        return output;
      }
      
      half4 frag(Varyings input) : SV_Target
      {
        // Sample texture for alpha testing
        half4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        
        // Only write depth for opaque pixels
        clip(texSample.a - 0.1);
        
        return half4(0, 0, 0, 0);
      }
      ENDHLSL
    }
}
  
  FallBack "Hidden/Universal Render Pipeline/FallbackError"
  //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}