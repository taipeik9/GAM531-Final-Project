using OpenTK.Mathematics;

namespace GAMFinalProject.Utility
{
    public class Camera
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2;

        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver2;

        // Third-person camera properties
        public Vector3 TargetPosition { get; set; } = Vector3.Zero; // The position the camera looks at (player position)
        public float Distance { get; set; } = 5.0f; // Distance behind the player
        public float HeightOffset { get; set; } = 2.0f; // Height offset above the player
        public float PlayerYaw { get; set; } = 0f; // The player's facing direction

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // The position of the camera
        public Vector3 Position { get; set; }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix for third-person camera
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, TargetPosition, _up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.1f, 100f);
        }

        // Update camera position to follow the target in third-person (fixed behind player)
        public void UpdateThirdPerson()
        {
            // Camera follows directly behind the player based on player's rotation
            // No pitch/yaw control - purely follows player
            float yawRad = PlayerYaw * (MathF.PI / 180f);

            // Calculate offset behind the player
            float offsetX = Distance * MathF.Sin(yawRad);
            float offsetZ = Distance * MathF.Cos(yawRad);

            // Position camera behind and above the player
            Position = new Vector3(
                TargetPosition.X - offsetX,
                TargetPosition.Y + HeightOffset,
                TargetPosition.Z - offsetZ
            );
        }

        // Get the forward direction based on player's facing (for movement input)
        public Vector3 GetForwardFromPlayer()
        {
            float yawRad = PlayerYaw * (MathF.PI / 180f);
            return new Vector3(MathF.Sin(yawRad), 0, MathF.Cos(yawRad)).Normalized();
        }

        // Get the right direction based on player's facing (for movement input)
        public Vector3 GetRightFromPlayer()
        {
            float yawRad = PlayerYaw * (MathF.PI / 180f);
            return new Vector3(MathF.Cos(yawRad), 0, -MathF.Sin(yawRad)).Normalized();
        }

        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}