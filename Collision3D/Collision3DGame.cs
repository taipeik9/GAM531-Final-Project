using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Collision3D
{
    class Collision3DGame : GameWindow
    {
        private int _shader;
        private int _modelLoc;
        private int _viewLoc;
        private int _projLoc;
        private PlayerController _player;
        private Camera _camera = new Camera();

        private readonly List<Entity> entities = new List<Entity>();

        public Collision3DGame(Vector2i windowSize, string title) :
            base(
                GameWindowSettings.Default,
                new NativeWindowSettings { ClientSize = windowSize, Title = title }
            )
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 1f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader = CreateShaderProgram();

            Entity playerEntity = new Entity(MeshFactory.CreateCube());
            playerEntity.Scale = new Vector3(0.5f, 0.5f, 0.5f);

            int crateTexture = Utils.LoadTexture(Constants.CrateTexturePath);
            playerEntity.Material = new Material(crateTexture);

            Entity cubeObstacle = new Entity(MeshFactory.CreateCube());
            int obstacleTexture = Utils.LoadTexture(Constants.MiscTexturePath);
            cubeObstacle.Material = new Material(obstacleTexture);
            cubeObstacle.SetPosition(new Vector3(2f, 0.25f, 0f));
            cubeObstacle.Scale = new Vector3(0.5f, 0.5f, 0.5f);

            entities.Add(cubeObstacle);

            _player = new PlayerController(playerEntity, _camera, new Vector3(-5, 0, -5), new Vector3(5, 5, 5));
            _player.Collidables.Add(cubeObstacle);

            entities.Add(playerEntity);

            foreach (Entity e in entities)
            {
                e.Mesh.UploadToGPU();
            }


            int floorTex = Utils.LoadTexture(Constants.FloorTexturePath);
            int ceilingTex = Utils.LoadTexture(Constants.CeilingTexturePath);
            int wallTex = Utils.LoadTexture(Constants.WallTexturePath);

            var roomBuilder = new RoomBuilder(
                width: 10f,
                height: 5f,
                depth: 10f,
                floorTexture: floorTex,
                ceilingTexture: ceilingTex,
                wallTexture: wallTex
            );

            var roomEntities = roomBuilder.Build();

            foreach (var e in roomEntities)
            {
                e.Mesh.UploadToGPU();
                entities.Add(e);
            }

            _modelLoc = GL.GetUniformLocation(_shader, "model");
            _viewLoc = GL.GetUniformLocation(_shader, "view");
            _projLoc = GL.GetUniformLocation(_shader, "projection");

            GL.UseProgram(_shader);
            int texLoc = GL.GetUniformLocation(_shader, "uTexture");
            if (texLoc != -1) GL.Uniform1(texLoc, 0);
        }

        private static int CreateShaderProgram()
        {
            string vertexString = File.ReadAllText(Constants.BaseVertexShaderPath);
            string fragmentString = File.ReadAllText(Constants.BaseFragmentShaderPath);

            int v = CompileShader(vertexString, ShaderType.VertexShader);
            int f = CompileShader(fragmentString, ShaderType.FragmentShader);

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string log = GL.GetProgramInfoLog(p);
                throw new Exception("PROGRAM LINK ERROR:\n" + log);
            }

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return p;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
            }

            _player.Update((float)args.Time, KeyboardState);

            float mouseDX = MouseState.Delta.X;
            float mouseDY = MouseState.Delta.Y;

            float sensitivity = 0.01f;

            _camera.Yaw += mouseDX * sensitivity;
            _camera.Pitch -= mouseDY * sensitivity;

            _camera.Pitch = Math.Clamp(_camera.Pitch, -1.2f, 0.7f);

            _camera.Update(_player.GetPosition());
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shader);

            Matrix4 view = _camera.ViewMatrix;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100f);

            GL.UniformMatrix4(_viewLoc, false, ref view);
            GL.UniformMatrix4(_projLoc, false, ref projection);

            foreach (Entity e in entities)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, e.Material.DiffuseTexture);

                Matrix4 model = e.ModelMatrix;
                GL.UniformMatrix4(_modelLoc, false, ref model);
                e.Mesh.Draw();
            }

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        private static int CompileShader(string shaderSrc, ShaderType type)
        {
            int s = GL.CreateShader(type);
            GL.ShaderSource(s, shaderSrc);
            GL.CompileShader(s);
            GL.GetShader(s, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0)
                throw new Exception($"SHADER COMPILE ERROR:\n{GL.GetShaderInfoLog(s)}");

            return s;
        }
    }
}