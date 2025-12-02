#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D tex;

void main()
{
    vec4 c = texture(tex, TexCoord);
    // Presume heart images have correct alpha; output directly
    FragColor = c;
}
