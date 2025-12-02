using System.Globalization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GAMFinalProject
{
    // Simple STL (binary or ASCII) model loader for OpenTK.
    // Provides a VAO/VBO with layout: position(3), normal(3), texCoord(2)
    public class StlModel : IDisposable
    {
        private readonly string _path;
        private int _vao;
        private int _vbo;
        private int _vertexCount;
        private bool _loaded;

        // Keep raw data so UVs can be regenerated without re-reading file
        // Format: X,Y,Z,NX,NY,NZ (we ignore the placeholder UVs returned by LoadStl)
        private List<float> _rawVertices = new();

        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;

        private Vector3 _minBounds = new(float.MaxValue);
        private Vector3 _maxBounds = new(float.MinValue);
        public Vector3 Center => (_minBounds + _maxBounds) * 0.5f;
        public Vector3 Size => _maxBounds - _minBounds;
        public int VertexCount => _vertexCount;

        // UV configuration
        //0 = XZ,1 = XY,2 = YZ,3 = Triplanar,4 = Cylindrical around Y axis
        public int UvMode { get; private set; } = 0;
        public Vector2 UvTiling { get; private set; } = new(1f, 1f);

        public StlModel(string path)
        {
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public void Load()
        {
            if (_loaded) return;
            if (!File.Exists(_path)) throw new FileNotFoundException("STL file not found", _path);

            _rawVertices = LoadStl(_path); // pattern X,Y,Z,NX,NY,NZ,0,0 (placeholder UV)

            // Compute bounds from positions
            for (int i = 0; i < _rawVertices.Count; i += 8)
            {
                float x = _rawVertices[i];
                float y = _rawVertices[i + 1];
                float z = _rawVertices[i + 2];
                if (x < _minBounds.X) _minBounds.X = x; if (y < _minBounds.Y) _minBounds.Y = y; if (z < _minBounds.Z) _minBounds.Z = z;
                if (x > _maxBounds.X) _maxBounds.X = x; if (y > _maxBounds.Y) _maxBounds.Y = y; if (z > _maxBounds.Z) _maxBounds.Z = z;
            }

            RebuildBuffers();
            _loaded = true;
        }

        public void SetUvMode(int mode, Vector2 tiling)
        {
            UvMode = Math.Clamp(mode, 0, 4);
            UvTiling = new Vector2(MathF.Max(tiling.X, 0.0001f), MathF.Max(tiling.Y, 0.0001f));
            if (_loaded)
            {
                RebuildBuffers();
            }
        }

        private void RebuildBuffers()
        {
            var size = Size;
            var rebuilt = new List<float>(_rawVertices.Count);
            for (int i = 0; i < _rawVertices.Count; i += 8)
            {
                var pos = new Vector3(_rawVertices[i], _rawVertices[i + 1], _rawVertices[i + 2]);
                var nrm = new Vector3(_rawVertices[i + 3], _rawVertices[i + 4], _rawVertices[i + 5]);
                var (u, v) = ComputeUV(pos, nrm, _minBounds, size, UvMode);
                u *= UvTiling.X; v *= UvTiling.Y;
                rebuilt.Add(pos.X); rebuilt.Add(pos.Y); rebuilt.Add(pos.Z);
                rebuilt.Add(nrm.X); rebuilt.Add(nrm.Y); rebuilt.Add(nrm.Z);
                rebuilt.Add(u); rebuilt.Add(v);
            }

            _vertexCount = rebuilt.Count / 8;

            if (_vao == 0)
            {
                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();
            }

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, rebuilt.Count * sizeof(float), rebuilt.ToArray(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); // position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1); // normal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(2); // texcoord
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            if (!_loaded) return;
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
        }

        private static (float u, float v) ComputeUV(Vector3 pos, Vector3 normal, Vector3 min, Vector3 size, int mode)
        {
            float sx = size.X <= 0 ? 1f : size.X;
            float sy = size.Y <= 0 ? 1f : size.Y;
            float sz = size.Z <= 0 ? 1f : size.Z;
            switch (mode)
            {
                case 1: // XY
                    return ((pos.X - min.X) / sx, (pos.Y - min.Y) / sy);
                case 2: // YZ
                    return ((pos.Z - min.Z) / sz, (pos.Y - min.Y) / sy);
                case 3: // Triplanar
                    {
                        var ax = MathF.Abs(normal.X);
                        var ay = MathF.Abs(normal.Y);
                        var az = MathF.Abs(normal.Z);
                        if (ax > ay && ax > az)
                        {
                            // Use YZ plane (project along X)
                            return ((pos.Z - min.Z) / sz, (pos.Y - min.Y) / sy);
                        }
                        else if (ay > ax && ay > az)
                        {
                            // Use XZ plane (project along Y)
                            return ((pos.X - min.X) / sx, (pos.Z - min.Z) / sz);
                        }
                        else
                        {
                            // Use XY plane (project along Z)
                            return ((pos.X - min.X) / sx, (pos.Y - min.Y) / sy);
                        }
                    }
                case 4: // Cylindrical around Y axis (good for pillars)
                    {
                        // Center in XZ
                        float cx = min.X + sx * 0.5f;
                        float cz = min.Z + sz * 0.5f;
                        float dx = pos.X - cx;
                        float dz = pos.Z - cz;
                        float theta = MathF.Atan2(dz, dx); // range -pi..pi
                        float u = (theta + MathF.PI) / (2f * MathF.PI); // normalize to0..1
                        float v = (pos.Y - min.Y) / sy; // vertical
                        return (u, v);
                    }
                default: //0: XZ
                    return ((pos.X - min.X) / sx, (pos.Z - min.Z) / sz);
            }
        }

        private static List<float> LoadStl(string path)
        {
            // Detect binary vs ASCII
            using var fs = File.OpenRead(path);
            if (IsBinaryStl(fs))
            {
                fs.Position = 0;
                return LoadBinary(fs);
            }
            fs.Position = 0;
            return LoadAscii(fs);
        }

        private static bool IsBinaryStl(FileStream fs)
        {
            if (fs.Length < 84) return false; // Minimum binary size
            byte[] header = new byte[80];
            fs.Read(header, 0, 80);
            byte[] countBytes = new byte[4];
            fs.Read(countBytes, 0, 4);
            uint triCount = BitConverter.ToUInt32(countBytes, 0);
            long expected = 80 + 4 + triCount * 50L; // each triangle50 bytes
            return expected == fs.Length; // Likely binary if sizes match
        }

        private static List<float> LoadBinary(Stream stream)
        {
            var vertices = new List<float>();
            using var br = new BinaryReader(stream);
            br.ReadBytes(80); // header
            uint triCount = br.ReadUInt32();
            for (uint i = 0; i < triCount; i++)
            {
                var normal = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v1 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v2 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                var v3 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                br.ReadUInt16(); // attribute byte count ignored
                AddVertex(vertices, v1, normal);
                AddVertex(vertices, v2, normal);
                AddVertex(vertices, v3, normal);
            }
            return vertices;
        }

        private static List<float> LoadAscii(Stream stream)
        {
            var vertices = new List<float>();
            using var sr = new StreamReader(stream);
            string? line;
            Vector3 currentNormal = Vector3.Zero;
            var faceVerts = new List<Vector3>(3);
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("facet normal", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    currentNormal = new Vector3(
                    float.Parse(parts[^3], CultureInfo.InvariantCulture),
                    float.Parse(parts[^2], CultureInfo.InvariantCulture),
                    float.Parse(parts[^1], CultureInfo.InvariantCulture));
                }
                else if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var v = new Vector3(
                    float.Parse(parts[^3], CultureInfo.InvariantCulture),
                    float.Parse(parts[^2], CultureInfo.InvariantCulture),
                    float.Parse(parts[^1], CultureInfo.InvariantCulture));
                    faceVerts.Add(v);
                }
                else if (line.StartsWith("endfacet", StringComparison.OrdinalIgnoreCase))
                {
                    if (faceVerts.Count == 3)
                    {
                        AddVertex(vertices, faceVerts[0], currentNormal);
                        AddVertex(vertices, faceVerts[1], currentNormal);
                        AddVertex(vertices, faceVerts[2], currentNormal);
                    }
                    faceVerts.Clear();
                }
            }
            return vertices;
        }

        private static void AddVertex(List<float> list, Vector3 pos, Vector3 normal)
        {
            // Placeholder UV values; real UVs generated later in Load()/RebuildBuffers based on selected mode
            list.Add(pos.X); list.Add(pos.Y); list.Add(pos.Z);
            list.Add(normal.X); list.Add(normal.Y); list.Add(normal.Z);
            list.Add(0f); list.Add(0f);
        }

        public void Dispose()
        {
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            _vbo = 0; _vao = 0; _loaded = false; _vertexCount = 0;
        }
    }
}
