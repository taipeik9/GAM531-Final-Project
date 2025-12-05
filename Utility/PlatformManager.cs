using OpenTK.Mathematics;

namespace GAMFinalProject
{
    public class PlatformManager : IDisposable
    {
        private List<Platform> _platforms = new();

        public struct PlatformDims(float Width, float Depth, float Height, float ScaleModifier)
        {
            public float width = Width;
            public float depth = Depth;
            public float height = Height;
            public float scaleModifier = ScaleModifier;
        }
        private void AddStaticPlatform(Vector3 position, PlatformDims dimensions)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Position = position,
                Width = dimensions.width,
                Height = dimensions.height,
                Depth = dimensions.depth,
                ScaleModifier = dimensions.scaleModifier,
                Type = PlatformType.Static
            };
            _platforms.Add(platform);
        }

        private void AddMovingPlatform(Vector3 startPos, Vector3 endPos, PlatformDims dimensions, float speed = 1.0f)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Width = dimensions.width,
                Height = dimensions.height,
                Depth = dimensions.depth,
                ScaleModifier = dimensions.scaleModifier,
            };
            platform.SetMovementPath(startPos, endPos, speed);
            _platforms.Add(platform);
        }

        public void SetupParkourCourse()
        {
            ClearPlatforms();
            PlatformDims easyPlat = new PlatformDims(0.75f, 0.75f, 0.2f, 1.4f);

            // Starting platform - much smaller collision to match visual
            AddStaticPlatform(
                position: new Vector3(0f, 0.5f, 4f),
                easyPlat
            );

            // Platform 2 - jump up
            AddStaticPlatform(
                position: new Vector3(-2f, 1.0f, 3f),
                easyPlat
            );

            // Platform 3 - higher jump
            AddStaticPlatform(
                position: new Vector3(-4f, 1.6f, 1.5f),
                easyPlat
            );

            // Moving platform - side to side
            AddMovingPlatform(
                startPos: new Vector3(-5.0f, 2.0f, 0.0f),
                endPos: new Vector3(-1.0f, 2.0f, 0.0f), // Changed X to -1.0f
                easyPlat,
                speed: 0.1f // Increased speed slightly for visibility
            );

            // Platform 4
            AddStaticPlatform(
                position: new Vector3(-1.0f, 2.5f, 0.5f),
                easyPlat
            );

            // Moving platform
            AddMovingPlatform(
                startPos: new Vector3(-1.0f, 3.0f, -0.5f),
                endPos: new Vector3(-1.0f, 3.0f, -3.0f),
                easyPlat,
                speed: 0.1f
            );

            // Moving platform
            AddMovingPlatform(
                startPos: new Vector3(-1.0f, 3.4f, -3.0f),
                endPos: new Vector3(-5.0f, 3.4f, -3.0f),
                easyPlat,
                speed: 0.1f
            );

            // Final goal platforms
            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.0f, -2.5f),
                easyPlat
            );

            // Extra platforms for more parkour
            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.6f, -1.5f),
                easyPlat
            );

            AddStaticPlatform(
                position: new Vector3(-5.0f, 5.2f, -0.5f),
                easyPlat
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