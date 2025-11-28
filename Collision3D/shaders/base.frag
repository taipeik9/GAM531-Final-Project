#version 330 core

in vec2 vUV;
in vec3 vNormal;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, vUV);
}
