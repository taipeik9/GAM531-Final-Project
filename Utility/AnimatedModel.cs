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
        public void Update(float deltaTime)
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
        public void Draw()
        {
            if (_currentAnimation == null || _currentAnimation.Frames.Count == 0)
                return;

            var currentFrame = _currentAnimation.Frames[_currentFrameIndex];

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