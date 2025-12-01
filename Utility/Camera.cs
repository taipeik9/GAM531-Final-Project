using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GAMFinalProject
{
    class Camera
    {
        public float Yaw = 0f;
        public float Pitch = 0f;
        public float Distance = 3f;
        public float AspectRatio;

        public Vector3 Position { get; private set; }
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        public Camera(float distance, float aspectRatio)
        {
            Distance = distance;
            AspectRatio = aspectRatio;
        }
        public void Update(Vector3 playerPos)
        {
            Vector3 dir = new Vector3(
                MathF.Sin(Yaw) * MathF.Cos(Pitch),
                MathF.Sin(Pitch),
                -MathF.Cos(Yaw) * MathF.Cos(Pitch)
            );

            dir = Vector3.Normalize(dir);

            Position = playerPos
                     - dir * Distance
                     + Vector3.UnitY;

            ViewMatrix = Matrix4.LookAt(Position, playerPos, Vector3.UnitY);

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), AspectRatio, 0.1f, 100f);
        }
    }
}