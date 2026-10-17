using System;
using System.IO;
using System.Threading.Tasks;

namespace DES_DEAL.Demo
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            string root = args.Length > 0 ? args[0] : AppContext.BaseDirectory;
            string testDataDirectory = Path.Combine(root, "TestData");
            Directory.CreateDirectory(testDataDirectory);
            var desRunner = new DesDemoRunner(testDataDirectory, Path.Combine(root, "Output", "DES"));
            var dealRunner = new DealDemoRunner(testDataDirectory,Path.Combine(root, "Output", "DEAL"));

            try
            {
                await desRunner.RunAsync();
                await dealRunner.RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"ERRROR: {ex}");
                Console.ResetColor();
                return 1;
            }

            return desRunner.AllPassed && dealRunner.AllPassed ? 0 : 1;
        }
    }
}