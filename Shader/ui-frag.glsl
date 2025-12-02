#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D tex;
uniform vec2 tiling;

void main()
{
    vec4 c = texture(tex, TexCoord * tiling);
    FragColor = c;
}
