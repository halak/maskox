Shader "Maskox/Linear"
{
    Properties
    {
        _Maskox_Angle ("Angle", Range (0, 360)) = 0
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
            #pragma fragment MaskoxFragmentShaderU
            ENDCG
        }
    }
}
