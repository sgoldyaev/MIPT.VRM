using OpenTK.Windowing.Desktop;

namespace MIPT.VRM.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = NativeWindowSettings.Default;

            using (VrmWindow game = new VrmWindow(gameWindowSettings, nativeWindowSettings))
                game.Run();
        }
    }
}
