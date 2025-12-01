#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform int texToUse = 0; // 0 = wall, 1 = player

void main()
{
    vec3 baseColor = texture(texture0, TexCoord).rgb;
    if (texToUse == 1)
    {
        baseColor = texture(texture1, TexCoord).rgb;
    }
    FragColor = vec4(baseColor, 1.0);
}
