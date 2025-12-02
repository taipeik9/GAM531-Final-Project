#version 330 core

layout(location = 0) in vec2 aPos; // unit quad coords (0..1)
layout(location = 1) in vec2 aTex;

uniform mat4 projection; // orthographic projection (pixels)
uniform mat4 model; // places/scales quad in pixel coords

out vec2 TexCoord;

void main()
{
    vec4 worldPos = model * vec4(aPos, 0.0, 1.0);
    gl_Position = projection * worldPos;
    TexCoord = aTex;
}
