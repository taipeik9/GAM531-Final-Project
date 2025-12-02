#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform int texToUse = 0; // 0 = wall, 1 = player, 2 = platform

// phong lighting parameters
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;
uniform float shininess = 32.0;
uniform float ambientStrength = 0.15;
uniform float specularStrength = 0.5;

void main()
{
    vec3 baseColor = texture(texture0, TexCoord).rgb;
    if (texToUse == 1)
    {
        baseColor = texture(texture1, TexCoord).rgb;
    }
    else if (texToUse == 2)
    {
        baseColor = texture(texture2, TexCoord).rgb;
    }

    // normal and light calculations
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    vec3 ambient = ambientStrength * lightColor;

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
    vec3 specular = specularStrength * spec * lightColor;

    vec3 lighting = ambient + diffuse + specular;

    vec3 result = lighting * baseColor;
    FragColor = vec4(result, 1.0);
}
