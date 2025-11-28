using OpenTK.Mathematics;

namespace Collision3D
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public Vertex(Vector3 pos, Vector3 normal, Vector2 uv)
        {
            Position = pos;
            Normal = normal;
            UV = uv;
        }

        public const int SizeInBytes = sizeof(float) * (3 + 3 + 2); // position + normal + uv
    }
}