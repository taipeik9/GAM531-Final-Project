using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Collision3D
{
    class PlayerController
    {
        private readonly Entity _entity;
        private readonly Camera _camera;
        public float MoveSpeed = 4.0f;
        public float Gravity = -9.81f;
        private float _verticalVelocity = 0f;

        public Vector3 RoomMin;
        public Vector3 RoomMax;

        public List<Entity> Collidables = new List<Entity>();
        public float JumpVelocity = 5f;

        public PlayerController(Entity entity, Camera camera, Vector3 roomMin, Vector3 roomMax)
        {
            _entity = entity;
            _camera = camera;
            RoomMin = roomMin;
            RoomMax = roomMax;
        }

        public void Update(float dt, KeyboardState input)
        {
            Vector3 move = Vector3.Zero;
            Vector3 forward = new Vector3(MathF.Sin(_camera.Yaw), 0, -MathF.Cos(_camera.Yaw));
            Vector3 left = new Vector3(forward.Z, 0, -forward.X);

            if (input.IsKeyDown(Keys.W)) move += forward;
            if (input.IsKeyDown(Keys.S)) move -= forward;
            if (input.IsKeyDown(Keys.A)) move += left;
            if (input.IsKeyDown(Keys.D)) move -= left;

            Vector3 horizontalMove = Vector3.Zero;
            if (move.LengthSquared > 0f)
            {
                move = Vector3.Normalize(move);
                horizontalMove = move * MoveSpeed * dt;
            }

            if (input.IsKeyPressed(Keys.Space) && IsOnGround())
            {
                _verticalVelocity = JumpVelocity;
            }

            _verticalVelocity += Gravity * dt;
            Vector3 displacement = new Vector3(horizontalMove.X, _verticalVelocity * dt, horizontalMove.Z);

            TryMove(displacement);
        }

        private bool IsOnGround()
        {
            float halfY = _entity.Scale.Y * 0.5f;
            return Math.Abs(_entity.Position.Y - (RoomMin.Y + halfY)) < 0.01f ||
                   Collidables.Exists(e => AABBCheck(_entity.Position - new Vector3(0, 0.01f, 0), _entity.Scale, e.Position, e.Scale));
        }

        private void TryMove(Vector3 displacement)
        {
            Vector3 newPos = _entity.Position;
            Vector3 halfScale = _entity.Scale * 0.5f;

            newPos.X += displacement.X;
            newPos.X = Math.Clamp(newPos.X, RoomMin.X + halfScale.X, RoomMax.X - halfScale.X);
            foreach (var e in Collidables)
            {
                if (AABBCheck(newPos, _entity.Scale, e.Position, e.Scale))
                    newPos.X = _entity.Position.X;
            }

            newPos.Y += displacement.Y;
            newPos.Y = Math.Clamp(newPos.Y, RoomMin.Y + halfScale.Y, RoomMax.Y - halfScale.Y);
            foreach (var e in Collidables)
            {
                if (AABBCheck(newPos, _entity.Scale, e.Position, e.Scale))
                {
                    if (displacement.Y > 0)
                        newPos.Y = e.Position.Y - halfScale.Y - e.Scale.Y * 0.5f;
                    else
                        newPos.Y = e.Position.Y + halfScale.Y + e.Scale.Y * 0.5f;

                    _verticalVelocity = 0f;
                }
            }

            newPos.Z += displacement.Z;
            newPos.Z = Math.Clamp(newPos.Z, RoomMin.Z + halfScale.Z, RoomMax.Z - halfScale.Z);
            foreach (var e in Collidables)
            {
                if (AABBCheck(newPos, _entity.Scale, e.Position, e.Scale))
                    newPos.Z = _entity.Position.Z;
            }

            _entity.SetPosition(newPos);
        }

        private bool AABBCheck(Vector3 posA, Vector3 scaleA, Vector3 posB, Vector3 scaleB)
        {
            Vector3 minA = posA - scaleA * 0.5f;
            Vector3 maxA = posA + scaleA * 0.5f;
            Vector3 minB = posB - scaleB * 0.5f;
            Vector3 maxB = posB + scaleB * 0.5f;

            return minA.X <= maxB.X && maxA.X >= minB.X &&
                   minA.Y <= maxB.Y && maxA.Y >= minB.Y &&
                   minA.Z <= maxB.Z && maxA.Z >= minB.Z;
        }

        public Vector3 GetPosition() => _entity.Position;
    }
}
