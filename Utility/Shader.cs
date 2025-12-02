using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            // ... (Shader loading, compilation, linking, and uniform caching remain the same) ...

            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, vertPath));

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fragPath));
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void Unload()
        {
            GL.UseProgram(0);
            GL.DeleteProgram(Handle);
        }

        // --- UPDATED UNIFORM SETTERS ---

        public void SetInt(string name, int data)
        {
            // Always call Use() before setting general uniforms for safety
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, data);
            }
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, data);
            }
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform3(location, data);
            }
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform2(location, data);
            }
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                // matrices are column-major; do not transpose when uploading to GLSL
                GL.UniformMatrix4(location, false, ref data);
            }
        }

        public void SetMatrix4NoUse(string name, Matrix4 data)
        {
            var location = GL.GetUniformLocation(Handle, name);

            if (location != -1)
            {
                GL.UniformMatrix4(location, false, ref data);
            }
        }
    }
}