using OpenTK.Mathematics;

namespace Collision3D
{
    static class MeshFactory
    {
        public static Mesh CreateCube(float size = 1f)
        {
            float s = size * 0.5f;

            Vertex[] v =
            [
                new Vertex(new Vector3(-s, -s,  s), Vector3.UnitZ, new Vector2(0,0)),
                new Vertex(new Vector3( s, -s,  s), Vector3.UnitZ, new Vector2(1,0)),
                new Vertex(new Vector3( s,  s,  s), Vector3.UnitZ, new Vector2(1,1)),
                new Vertex(new Vector3(-s,  s,  s), Vector3.UnitZ, new Vector2(0,1)),

                new Vertex(new Vector3( s, -s, -s), -Vector3.UnitZ, new Vector2(0,0)),
                new Vertex(new Vector3(-s, -s, -s), -Vector3.UnitZ, new Vector2(1,0)),
                new Vertex(new Vector3(-s,  s, -s), -Vector3.UnitZ, new Vector2(1,1)),
                new Vertex(new Vector3( s,  s, -s), -Vector3.UnitZ, new Vector2(0,1)),

                new Vertex(new Vector3(-s, -s, -s), -Vector3.UnitX, new Vector2(0,0)),
                new Vertex(new Vector3(-s, -s,  s), -Vector3.UnitX, new Vector2(1,0)),
                new Vertex(new Vector3(-s,  s,  s), -Vector3.UnitX, new Vector2(1,1)),
                new Vertex(new Vector3(-s,  s, -s), -Vector3.UnitX, new Vector2(0,1)),

                new Vertex(new Vector3( s, -s,  s), Vector3.UnitX, new Vector2(0,0)),
                new Vertex(new Vector3( s, -s, -s), Vector3.UnitX, new Vector2(1,0)),
                new Vertex(new Vector3( s,  s, -s), Vector3.UnitX, new Vector2(1,1)),
                new Vertex(new Vector3( s,  s,  s), Vector3.UnitX, new Vector2(0,1)),

                new Vertex(new Vector3(-s,  s,  s), Vector3.UnitY, new Vector2(0,0)),
                new Vertex(new Vector3( s,  s,  s), Vector3.UnitY, new Vector2(1,0)),
                new Vertex(new Vector3( s,  s, -s), Vector3.UnitY, new Vector2(1,1)),
                new Vertex(new Vector3(-s,  s, -s), Vector3.UnitY, new Vector2(0,1)),

                new Vertex(new Vector3(-s, -s, -s), -Vector3.UnitY, new Vector2(0,0)),
                new Vertex(new Vector3( s, -s, -s), -Vector3.UnitY, new Vector2(1,0)),
                new Vertex(new Vector3( s, -s,  s), -Vector3.UnitY, new Vector2(1,1)),
                new Vertex(new Vector3(-s, -s,  s), -Vector3.UnitY, new Vector2(0,1))
            ];

            uint[] indices = new uint[36];
            for (uint i = 0; i < 6; i++)
            {
                indices[i * 6 + 0] = i * 4 + 0;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                indices[i * 6 + 3] = i * 4 + 2;
                indices[i * 6 + 4] = i * 4 + 3;
                indices[i * 6 + 5] = i * 4 + 0;
            }

            return new Mesh(v, indices);
        }

        public static Mesh CreateQuad(float uRepeat = 1f, float vRepeat = 1f)
        {
            Vertex[] vertices =
            [
                new Vertex(new Vector3(-0.5f, -0.5f, 0f), new Vector3(0,0,1), new Vector2(0, 0)),
                new Vertex(new Vector3( 0.5f, -0.5f, 0f), new Vector3(0,0,1), new Vector2(uRepeat, 0)),
                new Vertex(new Vector3( 0.5f,  0.5f, 0f), new Vector3(0,0,1), new Vector2(uRepeat, vRepeat)),
                new Vertex(new Vector3(-0.5f,  0.5f, 0f), new Vector3(0,0,1), new Vector2(0, vRepeat))
            ];

            uint[] indices = [0, 1, 2, 2, 3, 0];
            return new Mesh(vertices, indices);
        }
    }
}