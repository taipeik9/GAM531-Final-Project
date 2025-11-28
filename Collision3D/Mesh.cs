using OpenTK.Graphics.OpenGL4;

namespace Collision3D
{
    class Mesh
    {
        public Vertex[] Vertices;
        public uint[] Indices;

        public int VertexCount => Vertices.Length;
        public int IndexCount => Indices.Length;

        private int _vao;
        private int _vbo;
        private int _ebo;
        public Mesh(Vertex[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void UploadToGPU()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexCount * Vertex.SizeInBytes, Vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            int stride = Vertex.SizeInBytes;

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}