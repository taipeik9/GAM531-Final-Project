using OpenTK.Mathematics;

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

            Vector3 position = playerPos
                     - dir * Distance
                     + Vector3.UnitY;

            // stop camera from ground clipping
            if (position.Y <= 0.1f)
            {
                position.Y = 0.1f;
            }

            ViewMatrix = Matrix4.LookAt(position, playerPos, Vector3.UnitY);
            Position = position;

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), AspectRatio, 0.1f, 100f);
        }
        public void ConstrainToRoom(Room room, float cameraRadius, Vector3 targetPosition)
        {
            if (room == null) return;

            var constrained = room.ConstrainCamera(Position, cameraRadius);
            if (constrained != Position)
            {
                Position = constrained;
                ViewMatrix = Matrix4.LookAt(Position, targetPosition, Vector3.UnitY);
            }
        }
    }
}