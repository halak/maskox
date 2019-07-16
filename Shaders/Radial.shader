Shader "Maskox/Radial"
{
    Properties
    {
        _Maskox_Center ("Center", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Default"
            CGPROGRAM
            #include "Maskox.cginc"
            #pragma vertex MaskoxVertexShaderRadial
            #pragma fragment MaskoxFragmentShaderUVLength
            ENDCG
        }
    }
}
