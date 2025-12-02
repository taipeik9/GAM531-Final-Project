using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    class UI
    {
        private readonly Texture _heartTexture;
        private readonly Texture _heartFadedTexture;
        private readonly int _uiVao;
        private readonly int _uiVbo;
        private readonly int _uiEbo;
        private const int HeartSize = 48; // pixels
        private const int HeartMargin = 10; // pixels from screen edge
        private readonly int MaxHearts;

        public UI(int maxHearts)
        {
            _heartTexture = Texture.LoadFromFile("Asset/heart.png");
            _heartFadedTexture = Texture.LoadFromFile("Asset/heart-faded.png");

            float[] quad = [
                0f, 0f, 0f, 1f,
                1f, 0f, 1f, 1f,
                1f, 1f, 1f, 0f,
                0f, 1f, 0f, 0f
            ];
            uint[] quadIdx = [0, 1, 2, 2, 3, 0];

            _uiVao = GL.GenVertexArray();
            _uiVbo = GL.GenBuffer();
            _uiEbo = GL.GenBuffer();

            GL.BindVertexArray(_uiVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _uiVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _uiEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, quadIdx.Length * sizeof(uint), quadIdx, BufferUsageHint.StaticDraw);

            int stride = 4 * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

            GL.BindVertexArray(0);

            MaxHearts = maxHearts;
        }
        public void Draw(Vector2 ClientSize, Shader shader, int currentHealth)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // orthographic projection with origin top-left
            var proj = Matrix4.CreateOrthographicOffCenter(0, ClientSize.X, ClientSize.Y, 0, -1f, 1f);
            shader.SetMatrix4("projection", proj);

            GL.BindVertexArray(_uiVao);

            float totalWidth = HeartSize * MaxHearts;
            float startX = ClientSize.X - HeartMargin - totalWidth;
            float y = HeartMargin + (HeartSize / 2);

            for (int i = 0; i < MaxHearts; i++)
            {
                float x = startX + i * HeartSize;
                var model = Matrix4.CreateScale(HeartSize, HeartSize, 1f) * Matrix4.CreateTranslation(x, y, 0f);
                shader.SetMatrix4("model", model);

                if (i < currentHealth)
                    _heartTexture.Use(TextureUnit.Texture0);
                else
                    _heartFadedTexture.Use(TextureUnit.Texture0);

                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindVertexArray(0);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        public void DrawTexture(Texture tex, Vector2 topLeft, Vector2 size, Shader shader, float scale = 1f, Vector2? tiling = null)
        {
            if (tex == null) return;
            shader.Use();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            var model = Matrix4.CreateScale(size.X * scale, size.Y * scale, 1f) *
                        Matrix4.CreateTranslation(topLeft.X - (size.X * (scale - 1f) / 2f), topLeft.Y - (size.Y * (scale - 1f) / 2f), 0f);

            shader.SetMatrix4("model", model);

            Vector2 t = tiling ?? Vector2.One;
            shader.SetVector2("tiling", t);

            tex.Use(TextureUnit.Texture0);
            shader.SetInt("tex", 0);
            GL.BindVertexArray(_uiVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_uiVbo);
            GL.DeleteBuffer(_uiEbo);
            GL.DeleteVertexArray(_uiVao);
        }
    }
}