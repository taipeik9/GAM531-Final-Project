using OpenTK.Mathematics;

namespace Collision3D
{
    class Entity
    {
        public Mesh Mesh;
        public Material Material;

        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        private bool _dirty = true;
        private Matrix4 _modelMatrix;

        public Entity(Mesh mesh, Vector3 position = default, Vector3 rotation = default, Vector3 scale = default)
        {
            Mesh = mesh;
            Position = position;
            Rotation = rotation;
            Scale = scale == default ? new Vector3(1f, 1f, 1f) : scale;
        }

        public Matrix4 ModelMatrix
        {
            get
            {
                if (_dirty)
                {
                    _modelMatrix = Matrix4.CreateScale(Scale) *
                                    Matrix4.CreateRotationX(Rotation.X) *
                                    Matrix4.CreateRotationY(Rotation.Y) *
                                    Matrix4.CreateTranslation(Position);

                    _dirty = false;
                }
                return _modelMatrix;
            }
        }

        public void SetPosition(Vector3 pos) { Position = pos; _dirty = true; }
        public void SetRotation(Vector3 rot) { Rotation = rot; _dirty = true; }
        public void SetScale(Vector3 scale) { Scale = scale; _dirty = true; }
    }
}