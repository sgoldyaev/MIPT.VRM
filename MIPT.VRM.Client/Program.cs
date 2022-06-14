namespace MIPT.VRM.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new AppClient();
            client.Start();

            Console.ReadLine();
        }
    }
}
