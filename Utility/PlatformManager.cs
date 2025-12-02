using OpenTK.Mathematics;

namespace GAMFinalProject
{
    public class PlatformManager : IDisposable
    {
        private List<Platform> _platforms = new();

        public void AddStaticPlatform(Vector3 position, float width, float height, float depth)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Position = position,
                Width = width,
                Height = height,
                Depth = depth,
                Type = PlatformType.Static
            };
            _platforms.Add(platform);
        }

        public void AddMovingPlatform(Vector3 startPos, Vector3 endPos, float width, float height, float depth, float speed = 1.0f)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Width = width,
                Height = height,
                Depth = depth
            };
            platform.SetMovementPath(startPos, endPos, speed);
            _platforms.Add(platform);
        }

        public void SetupParkourCourse()
        {
            ClearPlatforms();

            // Starting platform - much smaller collision to match visual
            AddStaticPlatform(
                position: new Vector3(0f, 0.5f, 4f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            // Platform 2 - jump up
            AddStaticPlatform(
                position: new Vector3(-2f, 1.0f, 3f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            // Platform 3 - higher jump
            AddStaticPlatform(
                position: new Vector3(-4f, 1.6f, 1.5f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            // Moving platform - side to side
            AddMovingPlatform(
                startPos: new Vector3(-5.0f, 2.0f, 0.0f),
                endPos: new Vector3(-1.0f, 2.0f, 0.0f), // Changed X to -1.0f
                width: 0.55f, height: 0.2f, depth: 0.55f,
                speed: 0.1f // Increased speed slightly for visibility
            );

            // Platform 4
            AddStaticPlatform(
                position: new Vector3(-1.0f, 2.5f, 0.5f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            // Moving platform
            AddMovingPlatform(
                startPos: new Vector3(-1.0f, 3.0f, -0.5f),
                endPos: new Vector3(-1.0f, 3.0f, -3.0f),
                width: 0.55f, height: 0.2f, depth: 0.55f,
                speed: 0.1f
            );

            // Moving platform
            AddMovingPlatform(
                startPos: new Vector3(-1.0f, 3.4f, -3.0f),
                endPos: new Vector3(-5.0f, 3.4f, -3.0f),
                width: 0.55f, height: 0.2f, depth: 0.55f,
                speed: 0.1f
            );

            // Final goal platforms
            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.0f, -2.5f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            // Extra platforms for more parkour
            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.6f, -1.5f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );

            AddStaticPlatform(
                position: new Vector3(-5.0f, 5.2f, -0.5f),
                width: 0.55f, height: 0.2f, depth: 0.55f
            );
        }

        public void Update(double deltaTime)
        {
            foreach (var platform in _platforms)
            {
                platform.Update(deltaTime);
            }
        }

        // Update the return signature to include Vector3 positionDelta
        public (bool, float, Vector3, Vector3) CheckPlayerOnPlatform(Vector3 playerPos, float playerRadius)
        {
            bool onGround = false;
            float highestSurface = float.MinValue;
            Vector3 platformVelocity = Vector3.Zero;
            Vector3 platformDelta = Vector3.Zero; // New variable

            foreach (var platform in _platforms)
            {
                if (platform.IsPlayerOnPlatform(playerPos, playerRadius, out float surfaceY))
                {
                    if (surfaceY > highestSurface)
                    {
                        highestSurface = surfaceY;
                        onGround = true;
                        platformVelocity = platform.GetVelocity();
                        platformDelta = platform.PositionDelta; // Capture the delta
                    }
                }
            }

            // Return 4 values now
            return (onGround, highestSurface, platformVelocity, platformDelta);
        }

        public bool CheckWallCollision(Vector3 fromPos, Vector3 toPos, float playerRadius)
        {
            foreach (var platform in _platforms)
            {
                if (platform.BlocksMovement(fromPos, toPos, playerRadius))
                {
                    return true;
                }
            }

            return false;
        }

        public void Draw(Shader shader)
        {
            foreach (var platform in _platforms)
            {
                platform.Draw(shader);
            }
        }

        public void ClearPlatforms()
        {
            foreach (var platform in _platforms)
            {
                platform.Dispose();
            }
            _platforms.Clear();
        }

        public void Dispose()
        {
            ClearPlatforms();
        }
    }
}