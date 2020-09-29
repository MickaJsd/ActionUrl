using System.IO;
namespace GeneratorConsoleUI
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            Run();

            Pause();
        }

        public static void Run()
        {
            AssemblyJsSerializer.ObjectMethodsSerializer serializer = new AssemblyJsSerializer.ObjectMethodsSerializer();
            var result = serializer.Serialize();

            System.Console.WriteLine(result);
        }

        private static void Pause()
        {
            System.Console.WriteLine("Pause...");
            System.Console.ReadLine();
        }
    }
}
