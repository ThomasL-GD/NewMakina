#ifndef SOBELOUTLINES_INCLUDED
#define SOBELOUTLINES_INCLUDED

static float2 sobelSamplePoints[9] = {
    float2(-1,1), float2(0,1), float2(1,1),
    float2(-1,0), float2(0,0), float2(1,0),
    float2(-1,-1), float2(0,-1), float2(1,-1)
};

static float sobelXMatrix[9] = {
    1,0,-1,
    2,0,-2,
    1,0,-1
};

static float sobelYMatrix[9] = {
    1,2,1,
    0,0,0,
    -1,-2,-1
};

void DepthSobel_float(float p_uv, float p_thickness, out float p_out)
{
    float2 sobel = 0;

    [unroll] for (int i =0; i<9; i++)
    {
        // float depth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(p_uv + sobelSamplePoints[i] * p_thickness);
        // sobel += depth * float2(sobelXMatrix[i],sobelYMatrix[i]);
    }

    p_out = length(sobel);
}

#endif