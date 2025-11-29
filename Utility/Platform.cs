using GAMFinalProject.Utility;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    public enum PlatformType
    {
        Static,
        Moving
    }

    public class Platform : IDisposable
    {
        private StlModel _model;

        // Visual properties
        public Vector3 Position { get; set; }
        public Vector3 VisualScale { get; set; } = new Vector3(0.01f, 0.01f, 0.01f);
        public Vector3 Rotation { get; set; } = Vector3.Zero;

        // Add this property to store the movement of the current frame
        public Vector3 PositionDelta { get; private set; } = Vector3.Zero;

        // Collision box (actual gameplay bounds)
        public float Width { get; set; } = 2.0f;   // X dimension
        public float Height { get; set; } = 0.3f;  // Y dimension
        public float Depth { get; set; } = 2.0f;   // Z dimension

        public PlatformType Type { get; set; } = PlatformType.Static;

        // For moving platforms
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public float MoveSpeed { get; set; } = 1.0f;
        private float _moveProgress = 0f;
        private bool _movingForward = true;

        public Matrix4 ModelMatrix { get; private set; } = Matrix4.Identity;

        public Platform(string modelPath)
        {
            _model = new StlModel(modelPath);
            _model.Load();
            _model.SetUvMode(1, new Vector2(1f, 1f));
        }

        public void SetMovementPath(Vector3 start, Vector3 end, float speed)
        {
            Type = PlatformType.Moving;
            StartPosition = start;
            EndPosition = end;
            Position = start;
            MoveSpeed = speed;
        }

        public void Update(double deltaTime)
        {
            // 1. Store where we were before moving
            Vector3 previousPosition = Position;

            if (Type == PlatformType.Moving)
            {
                _moveProgress += (float)deltaTime * MoveSpeed;

                // Ping-Pong logic (from previous fix)
                float cycle = _moveProgress % 2.0f;
                float t = cycle > 1.0f ? 2.0f - cycle : cycle;

                Position = Vector3.Lerp(StartPosition, EndPosition, t);
            }

            // 2. Calculate exactly how much we moved
            PositionDelta = Position - previousPosition;

            UpdateModelMatrix();
        }

        // Update GetVelocity to be more accurate (optional but recommended)
        public Vector3 GetVelocity()
        {
            // Return actual velocity based on this frame's movement
            // Avoids dividing by zero if deltaTime is super small
            return PositionDelta * 60f; // Approximate for momentum
        }

        private void UpdateModelMatrix()
        {
            var scale = Matrix4.CreateScale(VisualScale);
            var rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90f)); // Fix model orientation
            var userRot = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                          Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                          Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            var translation = Matrix4.CreateTranslation(Position);

            ModelMatrix = scale * rotX * userRot * translation;
        }

        // Get the exact top surface Y coordinate at a given XZ position
        public float GetSurfaceY(float x, float z)
        {
            float baseY = Position.Y + Height * 0.5f;

            return baseY;
        }

        // Check if a point is within the platform's horizontal bounds
        public bool IsWithinHorizontalBounds(float x, float z, float margin = 0f)
        {
            float halfWidth = Width * 0.5f + margin;
            float halfDepth = Depth * 0.5f + margin;

            return x >= Position.X - halfWidth && x <= Position.X + halfWidth &&
                   z >= Position.Z - halfDepth && z <= Position.Z + halfDepth;
        }

        // Check if a sphere (player) is on top of this platform
        public bool IsPlayerOnPlatform(Vector3 playerPos, float playerRadius, out float surfaceY)
        {
            surfaceY = 0f;

            // Check horizontal overlap with NO margin - player must be actually on the platform
            if (!IsWithinHorizontalBounds(playerPos.X, playerPos.Z, 0.05f))
                return false;

            // Get surface height at player position
            surfaceY = GetSurfaceY(playerPos.X, playerPos.Z);

            // Check if player is close to the surface
            float playerBottom = playerPos.Y - playerRadius;
            float distanceToSurface = playerBottom - surfaceY;

            // Player is on platform if they're within a small threshold above the surface
            // Use tighter range so player doesn't get "stuck" to platform when jumping
            return distanceToSurface >= -0.1f && distanceToSurface <= 0.15f;
        }

        // Check if movement would collide with platform sides (wall collision)
        public bool BlocksMovement(Vector3 fromPos, Vector3 toPos, float playerRadius)
        {
            // Use NO margin for wall collision - only block if actually inside the platform
            if (!IsWithinHorizontalBounds(toPos.X, toPos.Z, 0f))
                return false;

            // Get platform top at the new position
            float surfaceY = GetSurfaceY(toPos.X, toPos.Z);
            float platformBottom = Position.Y - Height * 0.5f;

            // If player is moving upward (jumping), allow them to pass through
            if (toPos.Y > fromPos.Y)
                return false;

            // If player's bottom is already above the platform top, don't block
            if (fromPos.Y - playerRadius > surfaceY)
                return false;

            // Check if player would be passing through the platform
            float playerBottom = toPos.Y - playerRadius;
            float playerTop = toPos.Y + playerRadius;

            // Block if player is trying to move through the platform from the side/below
            if (playerBottom < surfaceY && playerTop > platformBottom)
            {
                return true;
            }

            return false;
        }

        public void Draw(Shader shader)
        {
            shader.SetMatrix4("model", ModelMatrix);
            _model.Draw();
        }

        public void Dispose()
        {
            _model?.Dispose();
        }
    }
}