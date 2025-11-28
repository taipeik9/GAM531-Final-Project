using OpenTK.Mathematics;

namespace Collision3D
{
    class RoomBuilder
    {
        private float _width;
        private float _height;
        private float _depth;

        private int _floorTexture;
        private int _ceilingTexture;
        private int _wallTexture;

        public RoomBuilder(float width, float height, float depth, int floorTexture, int ceilingTexture, int wallTexture)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _floorTexture = floorTexture;
            _ceilingTexture = ceilingTexture;
            _wallTexture = wallTexture;
        }

        public List<Entity> Build()
        {
            var entities = new List<Entity>();

            // Floor (X,Z)
            var floorVerts = new Vertex[]
            {
                new Vertex(new Vector3(-_width/2, 0, -_depth/2), Vector3.UnitY, new Vector2(0, 0)),
                new Vertex(new Vector3( _width/2, 0, -_depth/2), Vector3.UnitY, new Vector2(_width, 0)),
                new Vertex(new Vector3( _width/2, 0,  _depth/2), Vector3.UnitY, new Vector2(_width, _depth)),
                new Vertex(new Vector3(-_width/2, 0,  _depth/2), Vector3.UnitY, new Vector2(0, _depth))
            };
            var floor = new Entity(new Mesh(floorVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            floor.Material = new Material(_floorTexture);
            entities.Add(floor);

            var ceilingVerts = new Vertex[]
            {
                new Vertex(new Vector3(-_width/2, _height, -_depth/2), -Vector3.UnitY, new Vector2(0, 0)),
                new Vertex(new Vector3( _width/2, _height, -_depth/2), -Vector3.UnitY, new Vector2(_width, 0)),
                new Vertex(new Vector3( _width/2, _height,  _depth/2), -Vector3.UnitY, new Vector2(_width, _depth)),
                new Vertex(new Vector3(-_width/2, _height,  _depth/2), -Vector3.UnitY, new Vector2(0, _depth))
            };
            var ceiling = new Entity(new Mesh(ceilingVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            ceiling.Material = new Material(_ceilingTexture);
            entities.Add(ceiling);

            var backWallVerts = new Vertex[]
            {
                new Vertex(new Vector3(-_width/2, 0, _depth/2), -Vector3.UnitZ, new Vector2(0, 0)),
                new Vertex(new Vector3(_width/2, 0, _depth/2), -Vector3.UnitZ, new Vector2(_width, 0)),
                new Vertex(new Vector3(_width/2, _height, _depth/2), -Vector3.UnitZ, new Vector2(_width, _height)),
                new Vertex(new Vector3(-_width/2, _height, _depth/2), -Vector3.UnitZ, new Vector2(0, _height))
            };
            var backWall = new Entity(new Mesh(backWallVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            backWall.Material = new Material(_wallTexture);
            entities.Add(backWall);

            var frontWallVerts = new Vertex[]
            {
                new Vertex(new Vector3(-_width/2, 0, -_depth/2), Vector3.UnitZ, new Vector2(0, 0)),
                new Vertex(new Vector3(_width/2, 0, -_depth/2), Vector3.UnitZ, new Vector2(_width, 0)),
                new Vertex(new Vector3(_width/2, _height, -_depth/2), Vector3.UnitZ, new Vector2(_width, _height)),
                new Vertex(new Vector3(-_width/2, _height, -_depth/2), Vector3.UnitZ, new Vector2(0, _height))
            };
            var frontWall = new Entity(new Mesh(frontWallVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            frontWall.Material = new Material(_wallTexture);
            entities.Add(frontWall);

            var leftWallVerts = new Vertex[]
            {
                new Vertex(new Vector3(-_width/2, 0, -_depth/2), -Vector3.UnitX, new Vector2(0, 0)),
                new Vertex(new Vector3(-_width/2, 0, _depth/2), -Vector3.UnitX, new Vector2(_depth, 0)),
                new Vertex(new Vector3(-_width/2, _height, _depth/2), -Vector3.UnitX, new Vector2(_depth, _height)),
                new Vertex(new Vector3(-_width/2, _height, -_depth/2), -Vector3.UnitX, new Vector2(0, _height))
            };
            var leftWall = new Entity(new Mesh(leftWallVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            leftWall.Material = new Material(_wallTexture);
            entities.Add(leftWall);

            var rightWallVerts = new Vertex[]
            {
                new Vertex(new Vector3(_width/2, 0, -_depth/2), Vector3.UnitX, new Vector2(0, 0)),
                new Vertex(new Vector3(_width/2, 0, _depth/2), Vector3.UnitX, new Vector2(_depth, 0)),
                new Vertex(new Vector3(_width/2, _height, _depth/2), Vector3.UnitX, new Vector2(_depth, _height)),
                new Vertex(new Vector3(_width/2, _height, -_depth/2), Vector3.UnitX, new Vector2(0, _height))
            };
            var rightWall = new Entity(new Mesh(rightWallVerts, new uint[] { 0, 1, 2, 2, 3, 0 }));
            rightWall.Material = new Material(_wallTexture);
            entities.Add(rightWall);

            foreach (var e in entities)
                e.Mesh.UploadToGPU();

            return entities;
        }
    }
}
