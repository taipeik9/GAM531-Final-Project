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
        // physics properites for movement
        public bool IsSprinting { get; set; } = true;
        // Physics properties for jumping
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public bool IsGrounded { get; set; } = true;
        public float GroundLevel { get; set; } = 0.5f;

        // physics constants
        private const float JumpForce = 7.0f;
        private const float Radius = 0.35f;
        private const float WalkSpeed = 2.0f;
        private const float SprintSpeed = 3.5f;
        private readonly float Gravity;
        // properties for damage
        private float? _initialFallHeight = null;
        public int Health { get; private set; }
        // attributes for footstep audio
        private const double FootStepTimerStart = 0.24f;
        private double _footstepTimer = FootStepTimerStart;
        private Random _footstepRand = new Random();

        public Player(AnimatedModel model, float gravity, int health)
        {
            _model = model;
            Gravity = gravity;
            Health = health;
        }

        public void Update(float dt, KeyboardState input, PlatformManager platformManager, Camera camera, Room room)
        {
            _model.Update(dt);

            // input Calculation (camera-relative)
            bool isMoving = false;
            Vector3 moveDirection = Vector3.Zero;

            // don't allow sprint start in air
            if (!IsSprinting && !IsGrounded)
            {
                IsSprinting = false;
            }
            else
            {
                IsSprinting = input.IsKeyDown(Keys.LeftShift);
            }

            float camYaw = camera.Yaw;
            float camPitch = camera.Pitch;
            var camForward = new Vector3(MathF.Sin(camYaw) * MathF.Cos(camPitch), 0f, -MathF.Cos(camYaw) * MathF.Cos(camPitch));
            if (camForward.LengthSquared > 0.0001f) camForward = Vector3.Normalize(camForward);

            var camRight = new Vector3(MathF.Cos(camYaw), 0f, MathF.Sin(camYaw));
            if (camRight.LengthSquared > 0.0001f) camRight = Vector3.Normalize(camRight);

            // determine movement direction
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

                horizontalMovement = moveNorm * (IsSprinting ? SprintSpeed : WalkSpeed) * dt;
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
                bool blockedByPlatform = platformManager.CheckWallCollision(Position, intendedPosition, Radius);

                if (blockedByPlatform)
                {
                    intendedPosition = Position + verticalMovement + platformMovement;
                }
                else if (room != null)
                {
                    var constrained = room.ConstrainMovement(Position, intendedPosition, Radius);
                    intendedPosition = constrained;
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
                    _model.PlayAnimation(isMoving ? IsSprinting ? "sprinting" : "walking" : "idle");
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
                        _model.PlayAnimation(isMoving ? IsSprinting ? "sprinting" : "walking" : "idle");
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
            // crazy stupid structure for animation
            // don't even ask about it lmao
            if (IsGrounded && _model.GetCurrentAnimationName() != "jumping")
            {
                if (isMoving)
                {
                    if (IsSprinting)
                    {
                        if (_model.GetCurrentAnimationName() != "sprinting")
                        {
                            _model.PlayAnimation("sprinting");
                        }
                    }
                    else
                    {
                        if (_model.GetCurrentAnimationName() != "walking")
                        {
                            _model.PlayAnimation("walking");
                        }
                    }
                }
                else if (!isMoving && (_model.GetCurrentAnimationName() == "walking" || _model.GetCurrentAnimationName() == "sprinting"))
                {
                    _model.PlayAnimation("idle");
                }
            }

            // Apply Rotation
            Rotation = new Vector3(-90f, _playerYaw, 0f);

            // check if player should take damage
            CheckFallHeight();
            UpdateFootsteps(isMoving, dt);
        }
        private void UpdateFootsteps(bool isMoving, float deltaTime)
        {
            if (!isMoving || !IsGrounded)
            {
                _footstepTimer = FootStepTimerStart;
                return;
            }

            double interval = IsSprinting ? FootStepTimerStart - 0.01 : 0.35;

            _footstepTimer += deltaTime;

            if (_footstepTimer >= interval)
            {
                _footstepTimer = 0;
                SoundEngine.PlayRandomFootstep(_footstepRand);
            }
        }

        private void CheckFallHeight()
        {
            if (!IsGrounded)
            {
                if (_initialFallHeight == null || Position.Y > _initialFallHeight)
                    _initialFallHeight = Position.Y;
            }
            else
            {
                if (_initialFallHeight - Position.Y >= 2.0f)
                {
                    TakeDamage();
                }
                _initialFallHeight = null;
            }
        }
        private void TakeDamage()
        {
            Health -= 1;
            SoundEngine.Play("break");
            SoundEngine.Play("damage");
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
        public void Reset(int health)
        {
            IsGrounded = true;
            IsSprinting = false;
            Health = health;
            _initialFallHeight = null;
            Velocity = Vector3.Zero;
            _playerYaw = 180f;
        }
        public void Dispose()
        {
            _model.Dispose();
        }
    }
}