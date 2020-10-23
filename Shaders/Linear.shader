Shader "Maskox/Linear"
{
    Properties
    {
        _Maskox_Angle ("Angle", Range (0, 360)) = 0
    }

    SubShader
    {
        Tags { "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Default"
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "Maskox.cginc"
            #pragma vertex MaskoxVertexShaderLinearGradient
            #pragma fragment MaskoxFragmentShaderU
            ENDCG
        }
    }
}
