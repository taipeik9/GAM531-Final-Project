using GAMFinalProject.Utility;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    // Represents a single animation (a sequence of STL frames)
    public class Animation
    {
        public string Name { get; set; }
        public List<StlModel> Frames { get; set; } = new();
        public float FrameRate { get; set; } = 10f; // frames per second
        public bool Loop { get; set; } = true; // Whether the animation loops

        public Animation(string name, float frameRate = 10f, bool loop = true)
        {
            Name = name;
            FrameRate = frameRate;
            Loop = loop;
        }
    }

    // Manages multiple animations and handles playback
    public class AnimatedModel : IDisposable
    {
        private Dictionary<string, Animation> _animations = new();
        private Animation? _currentAnimation;
        private int _currentFrameIndex = 0;
        private double _frameTimer = 0;

        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero; // Euler angles in degrees

        // Physics properties for jumping
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public bool IsGrounded { get; set; } = true;
        public float GroundLevel { get; set; } = 0.5f;

        public AnimatedModel()
        {
        }

        // Load an animation from a folder containing numbered STL files
        public void LoadAnimation(string animationName, string folderPath, int frameCount, float frameRate = 10f, bool loop = true)
        {
            var animation = new Animation(animationName, frameRate, loop);

            for (int i = 1; i <= frameCount; i++)
            {
                string filePath = Path.Combine(folderPath, $"{i}.stl");
                var model = new StlModel(filePath);
                model.Load();
                model.SetUvMode(2, new Vector2(1, 1));
                animation.Frames.Add(model);
            }

            _animations[animationName] = animation;

            // Set as current animation if it's the first one loaded
            if (_currentAnimation == null)
            {
                _currentAnimation = animation;
            }
        }

        // Switch to a different animation
        public void PlayAnimation(string animationName, bool restart = true)
        {
            if (_animations.TryGetValue(animationName, out var animation))
            {
                if (_currentAnimation != animation || restart)
                {
                    _currentAnimation = animation;
                    _currentFrameIndex = 0;
                    _frameTimer = 0;
                }
            }
        }

        // Update the animation (call this every frame)
        public void Update(double deltaTime)
        {
            if (_currentAnimation == null || _currentAnimation.Frames.Count == 0)
                return;

            _frameTimer += deltaTime;

            double frameDuration = 1.0 / _currentAnimation.FrameRate;

            if (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _currentFrameIndex++;

                // Handle looping or stopping at end
                if (_currentFrameIndex >= _currentAnimation.Frames.Count)
                {
                    if (_currentAnimation.Loop)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        _currentFrameIndex = _currentAnimation.Frames.Count - 1; // Stay on last frame
                    }
                }
            }
        }

        // Check if current animation has finished (for non-looping animations)
        public bool IsAnimationFinished()
        {
            if (_currentAnimation == null || _currentAnimation.Loop)
                return false;

            return _currentFrameIndex >= _currentAnimation.Frames.Count - 1;
        }

        // Set UV mode and tiling for all frames in all animations
        public void SetUvMode(int mode, Vector2 tiling)
        {
            foreach (var animation in _animations.Values)
            {
                foreach (var frame in animation.Frames)
                {
                    frame.SetUvMode(mode, tiling);
                }
            }
        }

        // Draw the current frame
        public void Draw(Shader shader)
        {
            if (_currentAnimation == null || _currentAnimation.Frames.Count == 0)
                return;

            var currentFrame = _currentAnimation.Frames[_currentFrameIndex];

            // Build transformation matrix: Scale -> Rotate -> Translate
            var scaleMatrix = Matrix4.CreateScale(Scale);

            // Apply rotations in order: X (pitch), Y (yaw), Z (roll)
            var rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
                                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
                                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));

            var translationMatrix = Matrix4.CreateTranslation(Position);

            // Original order: scale then rotate then translate (as in the user's initial code)
            ModelMatrix = scaleMatrix * rotationMatrix * translationMatrix;

            // Set the model matrix in the shader
            shader.SetMatrix4("model", ModelMatrix);

            // Draw the current frame
            currentFrame.Draw();
        }

        // Get the current frame's model (useful for collision detection, etc.)
        public StlModel? GetCurrentFrame()
        {
            if (_currentAnimation == null || _currentAnimation.Frames.Count == 0)
                return null;

            return _currentAnimation.Frames[_currentFrameIndex];
        }

        // Get info about current animation state
        public string GetCurrentAnimationName() => _currentAnimation?.Name ?? "None";
        public int GetCurrentFrameIndex() => _currentFrameIndex;
        public int GetCurrentAnimationFrameCount() => _currentAnimation?.Frames.Count ?? 0;

        public void Dispose()
        {
            foreach (var animation in _animations.Values)
            {
                foreach (var frame in animation.Frames)
                {
                    frame.Dispose();
                }
            }
            _animations.Clear();
        }
    }
}