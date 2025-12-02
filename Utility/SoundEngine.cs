using OpenTK.Audio.OpenAL;

namespace GAMFinalProject
{
    public static class SoundEngine
    {
        static Dictionary<string, int> _buffers = new();
        static Dictionary<string, int> _sources = new();
        static ALDevice _device;
        static ALContext _context;
        public static void Init()
        {
            _device = ALC.OpenDevice(null);
            if (_device == ALDevice.Null)
                throw new Exception("Failed to open OpenAL device.");

            _context = ALC.CreateContext(_device, (int[])null);
            if (_context == ALContext.Null)
                throw new Exception("Failed to create OpenAL context.");

            ALC.MakeContextCurrent(_context);

            AL.Listener(ALListener3f.Position, 0, 0, 1);
            AL.Listener(ALListenerf.Gain, 1f);

            Console.WriteLine("OpenAL ready");
        }


        public static void Load(string name, string path)
        {
            int buffer = AL.GenBuffer();
            int source = AL.GenSource();

            LoadWave(path, out int channels, out int bits, out int rate, out byte[] data);

            ALFormat format = channels switch
            {
                1 => bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16,
                2 => bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16,
                _ => throw new NotSupportedException("Unsupported format")
            };

            unsafe
            {

                fixed (byte* ptr = data)
                {
                    AL.BufferData(buffer, format, (nint)ptr, data.Length, rate);
                }
            }
            AL.Source(source, ALSourcei.Buffer, buffer);

            _buffers[name] = buffer;
            _sources[name] = source;
        }


        public static void Play(string name, bool loop = false, float volume = 1f)
        {
            if (!_sources.ContainsKey(name)) return;

            int src = _sources[name];
            AL.Source(src, ALSourceb.Looping, loop);
            AL.Source(src, ALSourcef.Gain, volume);
            AL.SourcePlay(src);
        }

        public static void Stop(string name)
        {
            if (!_sources.ContainsKey(name)) return;
            AL.SourceStop(_sources[name]);
        }

        public static bool IsPlaying(string name)
        {
            if (!_sources.ContainsKey(name)) return false;
            AL.GetSource(_sources[name], ALGetSourcei.SourceState, out int state);
            return state == (int)ALSourceState.Playing;
        }

        public static void SetVolume(string name, float volume)
        {
            AL.Source(_sources[name], ALSourcef.Gain, volume);
        }

        public static void Dispose()
        {
            foreach (var src in _sources.Values)
                AL.DeleteSource(src);

            foreach (var buf in _buffers.Values)
                AL.DeleteBuffer(buf);

            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(_context);
            ALC.CloseDevice(_device);
        }

        // WAV loader
        private static void LoadWave(string path, out int channels,
            out int bits, out int rate, out byte[] data)
        {
            using var reader = new BinaryReader(File.Open(path, FileMode.Open));

            // RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") throw new NotSupportedException("Not a WAV file");

            reader.ReadInt32();

            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") throw new NotSupportedException("Not a WAV file");

            channels = 0;
            bits = 0;
            rate = 0;
            data = null;

            // read in chunks
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string chunk = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunk == "fmt ")
                {
                    int format = reader.ReadInt16();
                    channels = reader.ReadInt16();
                    rate = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt16();
                    bits = reader.ReadInt16();

                    if (format != 1)
                        throw new NotSupportedException($"Unsupported WAV format: {format}");

                    if (chunkSize > 16)
                        reader.ReadBytes(chunkSize - 16);
                }
                else if (chunk == "data")
                {
                    data = reader.ReadBytes(chunkSize);
                }
                else
                {
                    // skip unknown chunk
                    reader.ReadBytes(chunkSize);
                }

                if (data != null)
                    break;
            }

            if (data == null)
                throw new Exception("WAV data chunk not found");
        }

        public static void PlayRandomFootstep(Random random)
        {
            string[] footsteps = ["footstep-1", "footstep-2", "footstep-3", "footstep-4", "footstep-5",];

            string sound = footsteps[random.Next(footsteps.Length)];

            float pitch = 0.9f + (float)random.NextDouble() * 0.2f;
            float volume = 0.8f + (float)random.NextDouble() * 0.2f;

            int src = _sources[sound];

            AL.Source(src, ALSourcef.Pitch, pitch);
            AL.Source(src, ALSourcef.Gain, volume);

            AL.SourcePlay(src);
        }

        // debugging helpers
        public static void CheckALErr(string where)
        {
            ALError err = AL.GetError();
            if (err != ALError.NoError)
                Console.WriteLine($"AL Error after {where}: {err}");
            else
                Console.WriteLine($"AL OK after {where}");
        }

        public static void PrintSourceState(string name)
        {
            if (!_sources.ContainsKey(name)) { Console.WriteLine($"{name} not loaded"); return; }
            AL.GetSource(_sources[name], ALGetSourcei.SourceState, out int state);
            Console.WriteLine($"{name} state = {(ALSourceState)state} ({state})");
        }

    }

}