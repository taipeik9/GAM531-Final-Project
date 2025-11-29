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
        private double _time;

        private Texture _wall_texture;
        private Texture _player_texture;
        private Texture _platform_texture;

        private AnimatedModel _player;
        private PlatformManager _platformManager;

        private const float PlayerSpeed = 3.5f;
        private const float PlayerRotationSpeed = 300f;
        private const float Gravity = -18f;
        private const float JumpForce = 7.5f;
        private const float PlayerRadius = 0.35f;
        private float _playerYaw = 180f;

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
            _platform_texture = Texture.LoadFromFile("Asset/platform.jpg");

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            _shader.SetInt("texture2", 2);

            _player = new AnimatedModel();
            _player.LoadAnimation("idle", "Asset/PlayerAnimations/Idle", 4, frameRate: 8f, loop: true);
            _player.LoadAnimation("walking", "Asset/PlayerAnimations/Walking", 7, frameRate: 14f, loop: true);
            _player.LoadAnimation("jumping", "Asset/PlayerAnimations/Jumping", 7, frameRate: 14f, loop: false);
            _player.SetUvMode(0, new Vector2(1f, 1f));
            _player.Position = new Vector3(0f, 0.0f, 4.0f);
            _player.Scale = new Vector3(2f, 2f, 2f);
            _player.Rotation = new Vector3(-90f, _playerYaw, 0f);
            _player.GroundLevel = 0.0f;
            _player.PlayAnimation("idle");

            _platformManager = new PlatformManager();
            _platformManager.SetupParkourCourse();

            _camera = new Camera(new Vector3(0.0f, 0.5f, 4.5f), Size.X / (float)Size.Y);
            _camera.Distance = 1.0f;
            _camera.HeightOffset = 1.0f;
            _camera.TargetPosition = _player.Position;
            _camera.PlayerYaw = _playerYaw;

            CursorState = CursorState.Normal;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _time += 4.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);
            _shader.Use();

            _camera.TargetPosition = _player.Position;
            _camera.PlayerYaw = _playerYaw;
            _camera.UpdateThirdPerson();

            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

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

            _player.Update(e.Time);
            _platformManager.Update(e.Time);

            if (!IsFocused) return;

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

            // --- 1. Rotation Logic ---
            if (input.IsKeyDown(Keys.A))
            {
                _playerYaw += PlayerRotationSpeed * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.D))
            {
                _playerYaw -= PlayerRotationSpeed * (float)e.Time;
            }

            // Normalize Yaw to 0-360
            while (_playerYaw >= 360f) _playerYaw -= 360f;
            while (_playerYaw < 0f) _playerYaw += 360f;

            // --- 2. Input Calculation ---
            bool isMoving = false;
            Vector3 moveDirection = Vector3.Zero;
            var factor = 0f;

            if (input.IsKeyDown(Keys.W))
            {
                float yawRad = _playerYaw * (MathF.PI / 180f);
                moveDirection = new Vector3(MathF.Sin(yawRad), 0, MathF.Cos(yawRad));
                isMoving = true;
            }

            if (input.IsKeyDown(Keys.S))
            {
                float yawRad = _playerYaw * (MathF.PI / 180f);
                factor = 180f;
                moveDirection = new Vector3(-MathF.Sin(yawRad), 0, -MathF.Cos(yawRad));
                isMoving = true;
            }

            // --- 3. Jump Logic ---
            if (input.IsKeyPressed(Keys.Space) && _player.IsGrounded)
            {
                // Keep existing horizontal velocity (momentum), add vertical force
                _player.Velocity = new Vector3(_player.Velocity.X, JumpForce, _player.Velocity.Z);
                _player.IsGrounded = false;
                _player.PlayAnimation("jumping", restart: true);
            }

            // --- 4. Physics & Gravity ---
            if (!_player.IsGrounded)
            {
                _player.Velocity = new Vector3(
                    _player.Velocity.X,
                    _player.Velocity.Y + Gravity * (float)e.Time,
                    _player.Velocity.Z
                );
            }

            // --- 5. Movement Calculation ---

            // A. Player Input Movement (Walking)
            Vector3 horizontalMovement = Vector3.Zero;
            if (isMoving && moveDirection.LengthSquared > 0.01f)
            {
                horizontalMovement = moveDirection.Normalized() * PlayerSpeed * (float)e.Time;
            }

            // B. Vertical Movement (Gravity/Jump)
            Vector3 verticalMovement = new Vector3(0, _player.Velocity.Y * (float)e.Time, 0);

            // C. Moving Platform Locking (Kinematic)
            // Check if we are currently standing on a platform *before* we move
            var (wasOnPlatform, _, _, currentPlatformDelta) =
                _platformManager.CheckPlayerOnPlatform(_player.Position, PlayerRadius);

            Vector3 platformMovement = Vector3.Zero;

            // If grounded on a platform, we stick to it (move exactly as much as it moved)
            if (wasOnPlatform && _player.IsGrounded)
            {
                platformMovement = currentPlatformDelta;
            }
            // If in the air, we use Momentum (Velocity) instead
            else if (!_player.IsGrounded)
            {
                platformMovement = new Vector3(_player.Velocity.X, 0, _player.Velocity.Z) * (float)e.Time;
            }

            // Combine all movements
            Vector3 intendedPosition = _player.Position + horizontalMovement + verticalMovement + platformMovement;


            // --- 6. Wall Collision ---
            // We only check wall collision if we are grounded or falling (allows jumping up through platforms)
            if (_player.IsGrounded || _player.Velocity.Y <= 0)
            {
                if (_platformManager.CheckWallCollision(_player.Position, intendedPosition, PlayerRadius))
                {
                    // Collision detected! 
                    // Cancel horizontal input, but keep vertical and platform movement so we don't get stuck
                    intendedPosition = _player.Position + verticalMovement + platformMovement;
                }
            }

            // --- 7. Landing / Ground Check ---

            // Check if the NEW position lands on a platform
            var (isOnPlatform, platformSurfaceY, platformVelocity, _) =
                _platformManager.CheckPlayerOnPlatform(intendedPosition, PlayerRadius);

            if (isOnPlatform && _player.Velocity.Y <= 0) // Only land if falling or flat
            {
                // Snap to platform surface
                intendedPosition = new Vector3(intendedPosition.X, platformSurfaceY + PlayerRadius, intendedPosition.Z);
                _player.IsGrounded = true;

                // Transfer platform velocity to player (so momentum works if we jump off next frame)
                _player.Velocity = new Vector3(platformVelocity.X, 0, platformVelocity.Z);

                // Land animation
                if (_player.GetCurrentAnimationName() == "jumping" && _player.IsAnimationFinished())
                {
                    _player.PlayAnimation(isMoving ? "walking" : "idle");
                }
            }
            else
            {
                // Fallback: Check static floor (GroundLevel)
                if (intendedPosition.Y <= _player.GroundLevel)
                {
                    intendedPosition = new Vector3(intendedPosition.X, _player.GroundLevel, intendedPosition.Z);
                    _player.IsGrounded = true;

                    // Friction: Stop sliding when hitting static ground
                    _player.Velocity = Vector3.Zero;

                    // Land animation
                    if (_player.GetCurrentAnimationName() == "jumping" && _player.IsAnimationFinished())
                    {
                        _player.PlayAnimation(isMoving ? "walking" : "idle");
                    }
                }
                else
                {
                    _player.IsGrounded = false;
                }
            }

            // --- 8. Final Apply ---
            _player.Position = intendedPosition;

            // --- 9. Animation State Update ---
            if (_player.IsGrounded && _player.GetCurrentAnimationName() != "jumping")
            {
                if (isMoving && _player.GetCurrentAnimationName() != "walking")
                {
                    _player.PlayAnimation("walking");
                }
                else if (!isMoving && _player.GetCurrentAnimationName() == "walking")
                {
                    _player.PlayAnimation("idle");
                }
            }

            // Apply Rotation
            _player.Rotation = new Vector3(-90f, _playerYaw + factor, 0f);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.Distance = Math.Clamp(_camera.Distance - e.OffsetY * 0.5f, 2f, 10f);
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