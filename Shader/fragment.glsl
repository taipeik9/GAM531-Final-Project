#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform sampler2D texture4;
uniform int texToUse = 0; // 0 = wall, 1 = player, 2 = platform

// phong lighting parameters
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;

uniform float spotlightIntensity;

// spotlight direction
uniform vec3 lightDir;

// spotlight soft edge
uniform float innerCutoff;  // cos(inner angle)
uniform float outerCutoff;  // cos(outer angle)

// attenuation (distance falloff)
uniform float constant = 1.0;
uniform float linear = 0.05;
uniform float quadratic = 0.01;

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
    else if (texToUse == 3)
    {
        baseColor = texture(texture3, TexCoord).rgb;
    }
    else if (texToUse == 4)
    {
        baseColor = texture(texture4, TexCoord).rgb;
    }

    vec3 norm = normalize(Normal);

    // direction and distance
    vec3 L = normalize(lightPos - FragPos);
    float distance = length(lightPos - FragPos);

    // attenuation
    float attenuation = 
        1.0 / (constant + linear * distance + quadratic * distance * distance);

    // spotlight intensity
    float theta = dot(L, normalize(-lightDir));
    float epsilon = innerCutoff - outerCutoff;
    float intensity = clamp((theta - outerCutoff) / epsilon, 0.0, 1.0);
    intensity *= spotlightIntensity;

    // ambient (slightly affected by attenuation)
    vec3 ambient = ambientStrength * lightColor * attenuation;

    // diffuse
    float diff = max(dot(norm, L), 0.0);
    vec3 diffuse = diff * lightColor * intensity * attenuation;

    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-L, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
    vec3 specular = specularStrength * spec * lightColor * intensity * attenuation;

    vec3 lighting = ambient + diffuse + specular;

    vec3 result = lighting * baseColor;
    FragColor = vec4(result, 1.0);
}
