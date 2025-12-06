using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GAMFinalProject
{
    internal class Game : GameWindow
    {
        private enum GameState { Title, Playing, GameOver, Paused, Win }
        private GameState _state = GameState.Title;
        private Shader _shader;
        private Shader _uiShader;
        private Screen _titleScreen;
        private Screen _gameOverScreen;
        private Screen _pauseScreen;
        private Screen _gameEndScreen;
        private UI _userInterface;
        private Camera _camera;
        private Texture _wall_texture;
        private Texture _player_texture;
        private Player _player;
        private readonly Vector3 PlayerStartPosition = new Vector3(0f, 0.0f, 4.0f);
        private PlatformManager _platformManager;
        private readonly Room _room = new Room();
        private const float LightHeight = 6.0f;
        private const int PlayerHealth = 3;
        private int _playerLives = 3;
        private Vector3 _respawnPoint;
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

            _shader = new Shader("Shader/vertex.glsl", "Shader/fragment.glsl");
            _shader.Use();

            _wall_texture = Texture.LoadFromFile("Asset/painted-concrete.jpg");
            _player_texture = Texture.LoadFromFile("Asset/concrete.jpg");
            Texture platformTexture = Texture.LoadFromFile("Asset/concrete-dark.jpg");
            Texture platformCheckpointTexture = Texture.LoadFromFile("Asset/checkpoint.jpg");
            Texture platformFinalTexture = Texture.LoadFromFile("Asset/final.jpg");

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            _shader.SetInt("texture2", 2);
            _shader.SetInt("texture3", 3);
            _shader.SetInt("texture4", 4);

            _room.Load();

            AnimatedModel playerModel = new AnimatedModel();
            playerModel.LoadAnimation("idle", "Asset/PlayerAnimations/Idle", 15, frameRate: 10f, loop: true);
            playerModel.LoadAnimation("walking", "Asset/PlayerAnimations/Walking", 6, frameRate: 12f, loop: true);
            playerModel.LoadAnimation("sprinting", "Asset/PlayerAnimations/Running", 7, frameRate: 14f, loop: true);
            playerModel.LoadAnimation("jumping", "Asset/PlayerAnimations/Jumping", 7, frameRate: 14f, loop: false);
            playerModel.SetUvMode(0, new Vector2(1f, 1f));
            playerModel.PlayAnimation("idle");

            _player = new Player(playerModel, Gravity, PlayerHealth);
            _player.Position = PlayerStartPosition;
            _player.Scale = new Vector3(2f, 2f, 2f);
            _player.Rotation = new Vector3(-90f, 180f, 0f);
            _player.GroundLevel = 0.0f;

            _platformManager = new PlatformManager(platformTexture, platformCheckpointTexture, platformFinalTexture, PlayerStartPosition);
            _platformManager.SetupParkourCourse();

            _camera = new Camera(Size.X / (float)Size.Y);

            _uiShader = new Shader("Shader/ui-vert.glsl", "Shader/ui-frag.glsl");
            _uiShader.Use();
            _uiShader.SetInt("tex", 0);

            _userInterface = new UI(PlayerHealth);

            Texture titleBgTexture = Texture.LoadFromFile("Asset/title-bg.jpg");
            Texture titleTexture = Texture.LoadFromFile("Asset/title.png");
            Texture startButtonTexture = Texture.LoadFromFile("Asset/start.png");
            _titleScreen = new Screen(Size, _userInterface, titleBgTexture, titleTexture, startButtonTexture);

            Texture gameOverBgTexture = Texture.LoadFromFile("Asset/game-over-bg.jpg");
            Texture gameOverTexture = Texture.LoadFromFile("Asset/game-over.png");
            Texture retryButtonTexture = Texture.LoadFromFile("Asset/retry.png");
            Texture quitButtonTexture = Texture.LoadFromFile("Asset/quit-game.png");
            _gameOverScreen = new Screen(Size, _userInterface, gameOverBgTexture, gameOverTexture, retryButtonTexture, quitButtonTexture);

            Texture pauseTexture = Texture.LoadFromFile("Asset/paused.png");
            Texture resumeButtonTexture = Texture.LoadFromFile("Asset/resume.png");
            _pauseScreen = new Screen(Size, _userInterface, titleBgTexture, pauseTexture, resumeButtonTexture, quitButtonTexture);

            Texture youWinTexture = Texture.LoadFromFile("Asset/you-win.png");
            _gameEndScreen = new Screen(Size, _userInterface, titleBgTexture, youWinTexture, retryButtonTexture, quitButtonTexture);

            _state = GameState.Title;
            CursorState = CursorState.Normal;

            SoundEngine.Init();
            SoundEngine.Load("footstep-1", "Asset/Sounds/footsteps/1.wav");
            SoundEngine.Load("footstep-2", "Asset/Sounds/footsteps/2.wav");
            SoundEngine.Load("footstep-3", "Asset/Sounds/footsteps/3.wav");
            SoundEngine.Load("footstep-4", "Asset/Sounds/footsteps/4.wav");
            SoundEngine.Load("footstep-5", "Asset/Sounds/footsteps/5.wav");
            SoundEngine.Load("damage", "Asset/Sounds/hurt.wav");
            SoundEngine.Load("break", "Asset/Sounds/bone-break.wav");
            SoundEngine.Load("music", "Asset/Sounds/background.wav");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_state == GameState.Title)
            {
                _titleScreen.RenderFrame(_uiShader, Size);
            }
            else if (_state == GameState.GameOver)
            {
                _gameOverScreen.RenderFrame(_uiShader, Size);
            }
            else if (_state == GameState.Paused)
            {
                _pauseScreen.RenderFrame(_uiShader, Size);
            }
            else if (_state == GameState.Win)
            {
                _gameEndScreen.RenderFrame(_uiShader, Size);
            }
            else
            {
                if (!SoundEngine.IsPlaying("music"))
                {
                    SoundEngine.Play("music", true);
                }
                _shader.Use();

                _camera.Update(new Vector3(_player.Position.X, _player.Position.Y + 0.25f, _player.Position.Z));

                _camera.ConstrainToRoom(_room, 0.25f, new Vector3(_player.Position.X, _player.Position.Y + 0.25f, _player.Position.Z));

                _shader.SetMatrix4("view", _camera.ViewMatrix);
                _shader.SetMatrix4("projection", _camera.ProjectionMatrix);

                // _shader.SetVector3("lightPos", _player.Position + new Vector3(0f, LightHeight, 0f)); // our light is gonna stay above the player and follow them
                _shader.SetVector3("lightPos", _player.Position + new Vector3(0f, LightHeight, 0f)); // our light follows
                _shader.SetVector3("lightColor", new Vector3(1f, 1f, 1f));
                _shader.SetVector3("viewPos", _camera.Position);
                _shader.SetFloat("shininess", 32f);
                _shader.SetFloat("ambientStrength", 0.2f);
                _shader.SetFloat("specularStrength", 0.5f);

                // spotlight points downward
                _shader.SetVector3("lightDir", new Vector3(0, -1, 0));
                _shader.SetFloat("spotlightIntensity", 2.0f); // modifier for the spotlight intensity
                _shader.SetFloat("innerCutoff", MathF.Cos(MathHelper.DegreesToRadians(50f)));
                _shader.SetFloat("outerCutoff", MathF.Cos(MathHelper.DegreesToRadians(90f)));

                // Draw room
                _wall_texture.Use(TextureUnit.Texture0);
                _shader.SetInt("texToUse", 0);
                _room.Draw(_shader);

                // Draw platforms
                _platformManager.Draw(_shader);

                // Draw player
                _player_texture.Use(TextureUnit.Texture1);
                _shader.SetInt("texToUse", 1);
                _player.Draw(_shader);

                _uiShader.Use();
                _userInterface.Draw(Size, _uiShader, _player.Health, _playerLives);

            }
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) return;

            var input = KeyboardState;
            if (input.IsKeyDown(Keys.Escape))
            {
                _state = GameState.Paused;
                CursorState = CursorState.Normal;
            }

            _respawnPoint = _platformManager.GetCurrentCheckpointPosition();

            if (_player.Health <= 0)
            {
                _playerLives -= 1;
                _player.Reset(PlayerHealth);
                _player.Position = _respawnPoint;
            }

            if (_playerLives <= 0)
            {
                _state = GameState.GameOver;
                CursorState = CursorState.Normal;
            }

            if (_platformManager.CheckGameComplete())
            {
                _state = GameState.Win;
                CursorState = CursorState.Normal;
            }

            if (_state == GameState.Title)
            {
                if (_titleScreen.CheckMainButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    // start game
                    _state = GameState.Playing;
                    CursorState = CursorState.Grabbed;
                }
            }
            else if (_state == GameState.GameOver)
            {
                if (_gameOverScreen.CheckMainButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    // restart game
                    ResetGame();
                }
                else if (_gameOverScreen.CheckSecondaryButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    Close();
                }
            }
            else if (_state == GameState.Paused)
            {
                if (_pauseScreen.CheckMainButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    // resume game
                    _state = GameState.Playing;
                    CursorState = CursorState.Grabbed;
                }
                else if (_pauseScreen.CheckSecondaryButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    Close();
                }
            }
            if (_state == GameState.Win)
            {
                if (_gameEndScreen.CheckMainButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    // restart game
                    ResetGame();
                    _state = GameState.Playing;
                    _platformManager.SetGameRestart();
                }
                else if (_gameEndScreen.CheckSecondaryButtonClick(MouseState.Position, MouseState.IsButtonDown(MouseButton.Left)))
                {
                    Close();
                }
            }
            else
            {
                _platformManager.Update(e.Time);

                _player.Update((float)e.Time, input, _platformManager, _camera, _room);

                float mouseDX = MouseState.Delta.X;
                float mouseDY = MouseState.Delta.Y;

                float sensitivity = 0.01f;

                _camera.Yaw += mouseDX * sensitivity;
                _camera.Pitch -= mouseDY * sensitivity;

                _camera.Pitch = Math.Clamp(_camera.Pitch, -1.2f, 0.7f);

                _camera.Update(_player.Position);
                _camera.ConstrainToRoom(_room, 0.25f, _player.Position);
            }
        }

        private void ResetGame()
        {
            _player.Reset(PlayerHealth);
            _player.Position = PlayerStartPosition;
            _player.Scale = new Vector3(2f, 2f, 2f);
            _player.Rotation = new Vector3(-90f, 180f, 0f);
            _playerLives = 3;
            _respawnPoint = PlayerStartPosition;
            _state = GameState.Playing;
            CursorState = CursorState.Grabbed;
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
            _titleScreen.Resize(Size);
            _gameOverScreen.Resize(Size);
            _gameEndScreen.Resize(Size);
            _pauseScreen.Resize(Size);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            _shader.Unload();
            _uiShader.Unload();
            _userInterface.Dispose();
            _titleScreen.Dispose();
            _player?.Dispose();
            _platformManager?.Dispose();
            _room?.Dispose();
            base.OnUnload();
        }
    }
}