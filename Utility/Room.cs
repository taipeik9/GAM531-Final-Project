using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    class Room
    {
        private readonly float[] _vertices = new float[]
        {
            // Floor (y = 0)
            -10.0f, 0.0f,  10.0f,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,
             10.0f, 0.0f,  10.0f,  0.0f, 1.0f, 0.0f,  2.0f, 0.0f,
             10.0f, 0.0f, -10.0f,  0.0f, 1.0f, 0.0f,  2.0f, 2.0f,
            -10.0f, 0.0f, -10.0f,  0.0f, 1.0f, 0.0f,  0.0f, 2.0f,

            // Left wall (x = -10)
            -10.0f, 0.0f,  10.0f,  1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
            -10.0f, 0.0f, -10.0f,  1.0f, 0.0f, 0.0f,  2.0f, 0.0f,
            -10.0f, 8.0f, -10.0f,  1.0f, 0.0f, 0.0f,  2.0f, 1.0f,
            -10.0f, 8.0f,  10.0f,  1.0f, 0.0f, 0.0f,  0.0f, 1.0f,

            // Right wall (x = 10)
             10.0f, 0.0f,  10.0f, -1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
             10.0f, 8.0f,  10.0f, -1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
             10.0f, 8.0f, -10.0f, -1.0f, 0.0f, 0.0f,  2.0f, 1.0f,
             10.0f, 0.0f, -10.0f, -1.0f, 0.0f, 0.0f,  2.0f, 0.0f,

            // Front wall (z = 10)
            -10.0f, 0.0f,  10.0f,  0.0f, 0.0f, -1.0f,  0.0f, 0.0f,
            -10.0f, 8.0f,  10.0f,  0.0f, 0.0f, -1.0f,  0.0f, 1.0f,
             10.0f, 8.0f,  10.0f,  0.0f, 0.0f, -1.0f,  2.0f, 1.0f,
             10.0f, 0.0f,  10.0f,  0.0f, 0.0f, -1.0f,  2.0f, 0.0f,

            // Back wall (z = -10)
             10.0f, 0.0f, -10.0f,  0.0f, 0.0f,  1.0f,  0.0f, 0.0f,
             10.0f, 8.0f, -10.0f,  0.0f, 0.0f,  1.0f,  0.0f, 1.0f,
            -10.0f, 8.0f, -10.0f,  0.0f, 0.0f,  1.0f,  2.0f, 1.0f,
            -10.0f, 0.0f, -10.0f,  0.0f, 0.0f,  1.0f,  2.0f, 0.0f,
        };

        private readonly uint[] _indices = new uint[]
        {
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
        };

        private int _elementBufferObject;
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        // Derived bounds computed from _vertices in Load()
        private float _minX = float.MaxValue;
        private float _maxX = float.MinValue;
        private float _minZ = float.MaxValue;
        private float _maxZ = float.MinValue;
        private float _wallTop = float.MinValue;

        public void Load()
        {
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);

            _minX = float.MaxValue; _maxX = float.MinValue;
            _minZ = float.MaxValue; _maxZ = float.MinValue;
            _wallTop = float.MinValue;

            for (int i = 0; i < _vertices.Length; i += 8)
            {
                float x = _vertices[i + 0];
                float y = _vertices[i + 1];
                float z = _vertices[i + 2];

                if (x < _minX) _minX = x;
                if (x > _maxX) _maxX = x;
                if (z < _minZ) _minZ = z;
                if (z > _maxZ) _maxZ = z;
                if (y > _wallTop) _wallTop = y;
            }
        }

        public void Draw(Shader _shader)
        {
            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }

        public Vector3 ConstrainCamera(Vector3 camPos, float radius)
        {
            if (_wallTop == float.MinValue)
            {
                return camPos;
            }

            // prevent camera going below the floor
            float minY = 0.1f + radius;
            if (camPos.Y < minY) camPos.Y = minY;

            float clampedX = Math.Clamp(camPos.X, _minX + radius, _maxX - radius);
            float clampedZ = Math.Clamp(camPos.Z, _minZ + radius, _maxZ - radius);

            return new Vector3(clampedX, camPos.Y, clampedZ);
        }

        public bool BlocksMovement(Vector3 fromPos, Vector3 toPos, float radius)
        {
            if (_wallTop == float.MinValue)
            {
                return false;
            }

            if (toPos.X - radius < _minX) return true;
            if (toPos.X + radius > _maxX) return true;
            if (toPos.Z - radius < _minZ) return true;
            if (toPos.Z + radius > _maxZ) return true;

            return false;
        }

        public Vector3 ConstrainMovement(Vector3 fromPos, Vector3 toPos, float radius)
        {
            if (_wallTop == float.MinValue)
            {
                return toPos; // no bounds known
            }

            float clampedX = Math.Max(_minX + radius, Math.Min(_maxX - radius, toPos.X));
            float clampedZ = Math.Max(_minZ + radius, Math.Min(_maxZ - radius, toPos.Z));

            return new Vector3(clampedX, toPos.Y, clampedZ);
        }
    }
}