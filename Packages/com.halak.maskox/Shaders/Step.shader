Shader "Maskox/Step"
{
    Properties
    {
        _Maskox_Angle ("Angle", Range (0, 360)) = 0
        _Maskox_Threshold ("Threshold", Range (0, 1)) = 0.5
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Default"

            CGPROGRAM
            #include "Maskox.cginc"
            #pragma vertex MaskoxVertexShaderLinearGradient
            #pragma fragment MaskoxFragmentShaderThresholdedU
            ENDCG
        }
    }
}
