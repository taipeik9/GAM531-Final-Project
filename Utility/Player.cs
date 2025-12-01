using GAMFinalProject.Utility;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GAMFinalProject
{
    class Player
    {
        private readonly AnimatedModel _model;
        private Matrix4 _modelMatrix;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero; // Euler angles in degrees
        private float _playerYaw = 180f;

        // Physics properties for jumping
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public bool IsGrounded { get; set; } = true;
        public float GroundLevel { get; set; } = 0.5f;

        // physics constants
        private const float JumpForce = 7.5f;
        private const float Radius = 0.3f;
        private const float Speed = 3.5f;
        private readonly float Gravity;

        public Player(AnimatedModel model, float gravity)
        {
            _model = model;
            Gravity = gravity;
        }

        public void Update(float dt, KeyboardState input, PlatformManager platformManager, Camera camera)
        {
            _model.Update(dt);

            // input Calculation (camera-relative)
            bool isMoving = false;
            Vector3 moveDirection = Vector3.Zero;

            float camYaw = camera.Yaw;
            float camPitch = camera.Pitch;
            var camForward = new Vector3(MathF.Sin(camYaw) * MathF.Cos(camPitch), 0f, -MathF.Cos(camYaw) * MathF.Cos(camPitch));
            if (camForward.LengthSquared > 0.0001f) camForward = Vector3.Normalize(camForward);

            var camRight = new Vector3(MathF.Cos(camYaw), 0f, MathF.Sin(camYaw));
            if (camRight.LengthSquared > 0.0001f) camRight = Vector3.Normalize(camRight);

            // forward / backward based on camera
            if (input.IsKeyDown(Keys.W))
            {
                moveDirection += camForward;
                isMoving = true;
            }

            if (input.IsKeyDown(Keys.S))
            {
                moveDirection -= camForward;
                isMoving = true;
            }

            // Left / right now move relative to the camera and also rotate the player to face that direction
            if (input.IsKeyDown(Keys.A))
            {
                moveDirection -= camRight;
                isMoving = true;
            }

            if (input.IsKeyDown(Keys.D))
            {
                moveDirection += camRight;
                isMoving = true;
            }

            // Jump Logic
            if (input.IsKeyPressed(Keys.Space) && IsGrounded)
            {
                // Keep existing horizontal velocity (momentum), add vertical force
                Velocity = new Vector3(Velocity.X, JumpForce, Velocity.Z);
                IsGrounded = false;
                _model.PlayAnimation("jumping", restart: true);
            }

            // Physics & Gravity
            if (!IsGrounded)
            {
                Velocity = new Vector3(
                    Velocity.X,
                    Velocity.Y + Gravity * dt,
                    Velocity.Z
                );
            }

            // Movement Calculation

            // A. Player Input Movement (Walking)
            Vector3 horizontalMovement = Vector3.Zero;
            if (isMoving && moveDirection.LengthSquared > 0.0001f)
            {
                var moveNorm = moveDirection.Normalized();

                // Rotate player to face movement direction (immediately)
                float desiredYawRad = MathF.Atan2(moveNorm.X, moveNorm.Z); // matches player's sin/cos convention
                float desiredYawDeg = MathHelper.RadiansToDegrees(desiredYawRad);
                _playerYaw = desiredYawDeg;

                horizontalMovement = moveNorm * Speed * dt;
            }

            // B. Vertical Movement (Gravity/Jump)
            Vector3 verticalMovement = new Vector3(0, Velocity.Y * dt, 0);

            // C. Moving Platform Locking (Kinematic)
            // Check if we are currently standing on a platform *before* we move
            var (wasOnPlatform, _, _, currentPlatformDelta) =
                platformManager.CheckPlayerOnPlatform(Position, Radius);

            Vector3 platformMovement = Vector3.Zero;

            // If grounded on a platform, we stick to it (move exactly as much as it moved)
            if (wasOnPlatform && IsGrounded)
            {
                platformMovement = currentPlatformDelta;
            }
            // If in the air, we use Momentum (Velocity) instead
            else if (!IsGrounded)
            {
                platformMovement = new Vector3(Velocity.X, 0, Velocity.Z) * dt;
            }

            // Combine all movements
            Vector3 intendedPosition = Position + horizontalMovement + verticalMovement + platformMovement;


            // --- 6. Wall Collision ---
            // We only check wall collision if we are grounded or falling (allows jumping up through platforms)
            if (IsGrounded || Velocity.Y <= 0)
            {
                if (platformManager.CheckWallCollision(Position, intendedPosition, Radius))
                {
                    // Collision detected! 
                    // Cancel horizontal input, but keep vertical and platform movement so we don't get stuck
                    intendedPosition = Position + verticalMovement + platformMovement;
                }
            }

            // --- 7. Landing / Ground Check ---

            // Check if the NEW position lands on a platform
            var (isOnPlatform, platformSurfaceY, platformVelocity, _) =
                platformManager.CheckPlayerOnPlatform(intendedPosition, Radius);

            if (isOnPlatform && Velocity.Y <= 0) // Only land if falling or flat
            {
                // Snap to platform surface
                intendedPosition = new Vector3(intendedPosition.X, platformSurfaceY + Radius, intendedPosition.Z);
                IsGrounded = true;

                // Transfer platform velocity to player (so momentum works if we jump off next frame)
                Velocity = new Vector3(platformVelocity.X, 0, platformVelocity.Z);

                // Land animation
                if (_model.GetCurrentAnimationName() == "jumping" && _model.IsAnimationFinished())
                {
                    _model.PlayAnimation(isMoving ? "walking" : "idle");
                }
            }
            else
            {
                // Fallback: Check static floor (GroundLevel)
                if (intendedPosition.Y <= GroundLevel)
                {
                    intendedPosition = new Vector3(intendedPosition.X, GroundLevel, intendedPosition.Z);
                    IsGrounded = true;

                    // Friction: Stop sliding when hitting static ground
                    Velocity = Vector3.Zero;

                    // Land animation
                    if (_model.GetCurrentAnimationName() == "jumping" && _model.IsAnimationFinished())
                    {
                        _model.PlayAnimation(isMoving ? "walking" : "idle");
                    }
                }
                else
                {
                    IsGrounded = false;
                }
            }

            // --- 8. Final Apply ---
            Position = intendedPosition;

            // --- 9. Animation State Update ---
            if (IsGrounded && _model.GetCurrentAnimationName() != "jumping")
            {
                if (isMoving && _model.GetCurrentAnimationName() != "walking")
                {
                    _model.PlayAnimation("walking");
                }
                else if (!isMoving && _model.GetCurrentAnimationName() == "walking")
                {
                    _model.PlayAnimation("idle");
                }
            }

            // Apply Rotation
            Rotation = new Vector3(-90f, _playerYaw, 0f);
        }

        public float GetYaw()
        {
            return _playerYaw;
        }

        public void Draw(Shader shader)
        {
            var scaleMatrix = Matrix4.CreateScale(Scale);

            var rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));

            var translationMatrix = Matrix4.CreateTranslation(Position);

            _modelMatrix = scaleMatrix * rotationMatrix * translationMatrix;
            shader.SetMatrix4("model", _modelMatrix);
            _model.Draw();
        }

        public void Dispose()
        {
            _model.Dispose();
        }
    }
}