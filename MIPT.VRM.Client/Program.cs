using System.Globalization;
using OpenTK.Windowing.Desktop;

namespace MIPT.VRM.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = NativeWindowSettings.Default;

            using (VrmWindow game = new VrmWindow(gameWindowSettings, nativeWindowSettings))
                game.Run();
        }
    }
}
