﻿namespace MIPT.VRM.Common.Entities;

public class VrmObject
{
    public static readonly float[] Vertices =
    {
        // Position
        -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0f, // Front face
        0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0f,
        0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0f,
        0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0f,
        -0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0f,
        -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0f,

        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 0f,// Back face
        0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 0f,
        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0f,
        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0f,
        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0f,
        -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 0f,

        -0.5f,  0.5f,  0.5f, 1.0f, .5f, 0f, // Left face
        -0.5f,  0.5f, -0.5f, 1.0f, .5f, 0f,
        -0.5f, -0.5f, -0.5f, 1.0f, .5f, 0f,
        -0.5f, -0.5f, -0.5f, 1.0f, .5f, 0f,
        -0.5f, -0.5f,  0.5f, 1.0f, .5f, 0f,
        -0.5f,  0.5f,  0.5f, 1.0f, .5f, 0f,

        0.5f,  0.5f,  0.5f, .5f, 1.0f, 0f, // Right face
        0.5f,  0.5f, -0.5f, .5f, 1.0f, 0f,
        0.5f, -0.5f, -0.5f, .5f, 1.0f, 0f,
        0.5f, -0.5f, -0.5f, .5f, 1.0f, 0f,
        0.5f, -0.5f,  0.5f, .5f, 1.0f, 0f,
        0.5f,  0.5f,  0.5f, .5f, 1.0f, 0f,

        -0.5f, -0.5f, -0.5f, .5f, .5f, .5f, // Bottom face
        0.5f, -0.5f, -0.5f, .5f, .5f, .5f,
        0.5f, -0.5f,  0.5f, .5f, .5f, .5f,
        0.5f, -0.5f,  0.5f, .5f, .5f, .5f,
        -0.5f, -0.5f,  0.5f, .5f, .5f, .5f,
        -0.5f, -0.5f, -0.5f, .5f, .5f, .5f,

        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, .5f, // Top face
        0.5f,  0.5f, -0.5f, 1.0f, 1.0f, .5f,
        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, .5f,
        0.5f,  0.5f,  0.5f, 1.0f, 1.0f, .5f,
        -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, .5f,
        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, .5f,
    };
}
