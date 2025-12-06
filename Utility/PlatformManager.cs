using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    public class PlatformManager : IDisposable
    {
        private List<Platform> _platforms = new();
        private int _currentCheckpoint = 0;
        private bool _isGameComplete = false;
        private Dictionary<int, Vector3> _checkpointMap = [];
        private Texture _platform_texture;
        private Texture _checkpointPlatform_texture;
        private Texture _finalPlatform_texture;
        public PlatformManager(Texture platformTexture, Texture checkpointPlatformTexture, Texture finalPlatformTexture, Vector3 startPos)
        {
            _platform_texture = platformTexture;
            _checkpointPlatform_texture = checkpointPlatformTexture;
            _finalPlatform_texture = finalPlatformTexture;
            _checkpointMap[0] = startPos;
        }

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

        private void AddCheckpointPlatform(Vector3 position, PlatformDims dimensions)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Position = position,
                Width = dimensions.width,
                Height = dimensions.height,
                Depth = dimensions.depth,
                ScaleModifier = dimensions.scaleModifier,
                Type = PlatformType.CheckPoint
            };
            _platforms.Add(platform);
        }
        private void AddFinalPlatform(Vector3 position, PlatformDims dimensions)
        {
            var platform = new Platform("Asset/platform.stl")
            {
                Position = position,
                Width = dimensions.width,
                Height = dimensions.height,
                Depth = dimensions.depth,
                ScaleModifier = dimensions.scaleModifier,
                Type = PlatformType.Final
            };
            _platforms.Add(platform);
        }

        public void SetupParkourCourse()
        {
            ClearPlatforms();
            PlatformDims easyPlat = new PlatformDims(0.75f, 0.75f, 0.2f, 1.4f);
            PlatformDims hardPlat = new PlatformDims(0.33f, 0.33f, 0.2f, 0.8f);
            PlatformDims checkpointPlat = new PlatformDims(1f, 1f, 0.2f, 1.8f);

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
            AddCheckpointPlatform(
                position: new Vector3(-1.0f, 2.5f, 0.5f),
                checkpointPlat
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

            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.0f, -2.5f),
                easyPlat
            );

            AddStaticPlatform(
                position: new Vector3(-5.0f, 4.6f, -1.5f),
                easyPlat
            );

            AddCheckpointPlatform(
                position: new Vector3(-5.0f, 5.2f, -0.5f),
                easyPlat
            );

            // final push before goal
            AddStaticPlatform(
                position: new Vector3(-3.0f, 6.0f, -1.5f),
                hardPlat
            );

            AddStaticPlatform(
                position: new Vector3(0.0f, 6.8f, 0f),
                hardPlat
            );

            AddMovingPlatform(
                startPos: new Vector3(-3.0f, 6.4f, 1.0f),
                endPos: new Vector3(1.0f, 6.8f, -3.0f),
                hardPlat,
                0.3f
            );
            // goal
            AddFinalPlatform(
                position: new Vector3(1.0f, 7.2f, 1.0f),
                easyPlat
            );

            int checkpointCount = 0;
            foreach (var platform in _platforms)
            {
                if (platform.Type == PlatformType.CheckPoint)
                {
                    checkpointCount += 1;
                    platform.CheckpointNumber = checkpointCount;
                    _checkpointMap[checkpointCount] = platform.Position + new Vector3(0f, 1f, 0f);
                }
            }
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
                    if (platform.Type == PlatformType.CheckPoint && platform.CheckpointNumber > _currentCheckpoint)
                    {
                        _currentCheckpoint = platform.CheckpointNumber;
                    }
                    else if (platform.Type == PlatformType.Final)
                    {
                        _isGameComplete = true;
                    }
                }
            }

            // Return 4 values now
            return (onGround, highestSurface, platformVelocity, platformDelta);
        }

        public Vector3 GetCurrentCheckpointPosition()
        {
            return _checkpointMap[_currentCheckpoint];
        }

        public bool CheckGameComplete()
        {
            return _isGameComplete;
        }
        public void SetGameRestart()
        {
            _isGameComplete = false;
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
                if (platform.Type == PlatformType.CheckPoint)
                {
                    _checkpointPlatform_texture.Use(TextureUnit.Texture3);
                    shader.SetInt("texToUse", 3);
                }
                else if (platform.Type == PlatformType.Final)
                {
                    _finalPlatform_texture.Use(TextureUnit.Texture4);
                    shader.SetInt("texToUse", 4);
                }
                else
                {
                    _platform_texture.Use(TextureUnit.Texture2);
                    shader.SetInt("texToUse", 2);
                }
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