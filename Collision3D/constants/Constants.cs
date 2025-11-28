using OpenTK.Mathematics;

namespace Collision3D
{
    public static class Constants
    {
        public static readonly Vector2i WindowSize = new(800, 600);
        public const string CrateTexturePath = "assets/crate.png";
        public const string MiscTexturePath = "assets/obstacle.png";
        public const string FloorTexturePath = "assets/wall.png";
        public const string CeilingTexturePath = "assets/wall.png";
        public const string WallTexturePath = "assets/wall.png";
        public const string BaseVertexShaderPath = "shaders/base.vert";
        public const string BaseFragmentShaderPath = "shaders/base.frag";
    }
}