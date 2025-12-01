using GAMFinalProject.Utility;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GAMFinalProject
{
    internal class Game : GameWindow
    {
        private readonly float[] _vertices =
        {
            -7.0f, 0.0f,  6.0f,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,
             1.0f, 0.0f,  6.0f,  0.0f, 1.0f, 0.0f,  2.0f, 0.0f,
             1.0f, 0.0f, -3.0f,  0.0f, 1.0f, 0.0f,  2.0f, 2.0f,
            -7.0f, 0.0f, -3.0f,  0.0f, 1.0f, 0.0f,  0.0f, 2.0f,

            -7.0f, 0.0f,  6.0f,  1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
            -7.0f, 0.0f, -3.0f,  1.0f, 0.0f, 0.0f,  2.0f, 0.0f,
            -7.0f, 1.0f, -3.0f,  1.0f, 0.0f, 0.0f,  2.0f, 1.0f,
            -7.0f, 1.0f,  6.0f,  1.0f, 0.0f, 0.0f,  0.0f, 1.0f,

             1.0f, 0.0f,  6.0f, -1.0f, 0.0f, 0.0f,  0.0f, 0.0f,
             1.0f, 1.0f,  6.0f, -1.0f, 0.0f, 0.0f,  0.0f, 1.0f,
             1.0f, 1.0f, -3.0f, -1.0f, 0.0f, 0.0f,  2.0f, 1.0f,
             1.0f, 0.0f, -3.0f, -1.0f, 0.0f, 0.0f,  2.0f, 0.0f,

            -7.0f, 0.0f,  6.0f,  0.0f, 0.0f, -1.0f,  0.0f, 0.0f,
            -7.0f, 1.0f,  6.0f,  0.0f, 0.0f, -1.0f,  0.0f, 1.0f,
             1.0f, 1.0f,  6.0f,  0.0f, 0.0f, -1.0f,  2.0f, 1.0f,
             1.0f, 0.0f,  6.0f,  0.0f, 0.0f, -1.0f,  2.0f, 0.0f,

             1.0f, 0.0f, -3.0f,  0.0f, 0.0f,  1.0f,  0.0f, 0.0f,
             1.0f, 1.0f, -3.0f,  0.0f, 0.0f,  1.0f,  0.0f, 1.0f,
            -7.0f, 1.0f, -3.0f,  0.0f, 0.0f,  1.0f,  2.0f, 1.0f,
            -7.0f, 0.0f, -3.0f,  0.0f, 0.0f,  1.0f,  2.0f, 0.0f,
        };

        private readonly uint[] _indices =
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
        private Shader _shader;
        private Camera _camera;

        private Texture _wall_texture;
        private Texture _player_texture;
        private Texture _platform_texture;

        private Player _player;
        private PlatformManager _platformManager;

        private const float Gravity = -18f;
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.CullFace);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shader/vertex.glsl", "Shader/fragment.glsl");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            var normalLocation = _shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            _wall_texture = Texture.LoadFromFile("Asset/brick-wall.jpg");
            _player_texture = Texture.LoadFromFile("Asset/player.jpeg");
            _platform_texture = Texture.LoadFromFile("Asset/brick-wall.jpg");

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            _shader.SetInt("texture2", 2);

            AnimatedModel playerModel = new AnimatedModel();
            playerModel.LoadAnimation("idle", "Asset/PlayerAnimations/Walking", 1, frameRate: 1f, loop: true);
            playerModel.LoadAnimation("walking", "Asset/PlayerAnimations/Walking", 6, frameRate: 12f, loop: true);
            playerModel.LoadAnimation("jumping", "Asset/PlayerAnimations/Jumping", 7, frameRate: 14f, loop: false);
            playerModel.SetUvMode(0, new Vector2(1f, 1f));
            playerModel.PlayAnimation("idle");

            _player = new Player(playerModel, Gravity);
            _player.Position = new Vector3(0f, 0.0f, 4.0f);
            _player.Scale = new Vector3(2f, 2f, 2f);
            _player.Rotation = new Vector3(-90f, _player.GetYaw(), 0f);
            _player.GroundLevel = 0.0f;

            _platformManager = new PlatformManager();
            _platformManager.SetupParkourCourse();

            _camera = new Camera(3f, Size.X / (float)Size.Y);
            // Capture the cursor so mouse movement controls the camera
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);
            _shader.Use();

            _camera.Update(new Vector3(_player.Position.X, _player.Position.Y + 0.25f, _player.Position.Z));

            _shader.SetMatrix4("view", _camera.ViewMatrix);
            _shader.SetMatrix4("projection", _camera.ProjectionMatrix);

            // Draw walls
            _wall_texture.Use(TextureUnit.Texture0);
            _shader.SetInt("texToUse", 0);
            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            // Draw platforms
            _platform_texture.Use(TextureUnit.Texture2);
            _shader.SetInt("texToUse", 2);
            _platformManager.Draw(_shader);

            // Draw player
            _player_texture.Use(TextureUnit.Texture1);
            _shader.SetInt("texToUse", 1);
            _player.Draw(_shader);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _platformManager.Update(e.Time);

            if (!IsFocused) return;

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

            _player.Update((float)e.Time, input, _platformManager, _camera);

            float mouseDX = MouseState.Delta.X;
            float mouseDY = MouseState.Delta.Y;

            float sensitivity = 0.01f;

            _camera.Yaw += mouseDX * sensitivity;
            _camera.Pitch -= mouseDY * sensitivity;

            _camera.Pitch = Math.Clamp(_camera.Pitch, -1.2f, 0.7f);

            _camera.Update(_player.Position);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.Distance = Math.Clamp(_camera.Distance - e.OffsetY * 0.5f, 2f, 10f);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            float sensitivity = 0.01f;

            float mouseDX = MouseState.Delta.X;
            float mouseDY = MouseState.Delta.Y;


            _camera.Yaw += mouseDX * sensitivity;
            _camera.Pitch -= mouseDY * sensitivity;

            _camera.Pitch = Math.Clamp(_camera.Pitch, -1.2f, 0.7f);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vertexArrayObject);
            _shader.Unload();
            _player?.Dispose();
            _platformManager?.Dispose();
            base.OnUnload();
        }
    }
}