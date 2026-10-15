using System;
using System.IO;
using System.Threading.Tasks;

namespace DES_DEAL.Demo
{
    /// <summary>
    /// Точка входа демонстрационной консольной программы (задание 5).
    /// </summary>
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // По умолчанию ищем TestData рядом со скомпилированным .dll (csproj копирует
            // её туда при сборке - см. CryptoLab.Demo.csproj). Можно переопределить первым
            // аргументом командной строки: dotnet run -- "C:\путь\к\своей\папке".
            string testDataDirectory = args.Length > 0
                ? args[0]
                : Path.Combine(AppContext.BaseDirectory, "TestData");

            string outputDirectory = Path.Combine(AppContext.BaseDirectory, "Output");

            var runner = new DemoRunner(testDataDirectory, outputDirectory);

            try
            {
                await runner.RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"Демонстрация прервалась необработанной ошибкой: {ex}");
                Console.ResetColor();
                return 1;
            }

            return runner.AllPassed ? 0 : 1;
        }
    }
}