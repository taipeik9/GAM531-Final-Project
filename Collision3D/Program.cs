namespace Collision3D
{
    class Program
    {
        public static void Main()
        {
            using Collision3DGame game = new(Constants.WindowSize, "3D Collision Game");
            game.Run();
        }
    }
}