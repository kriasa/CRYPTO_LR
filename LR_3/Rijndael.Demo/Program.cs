
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rijndael.Demo
{
    public static class Program
    {
        public static async Task Main()
        {
            string baseDirectory = AppContext.BaseDirectory;

            string testDataDirectory = Path.Combine(baseDirectory, "TestData");
            string outputDirectory = Path.Combine(baseDirectory, "Output");

            Directory.CreateDirectory(outputDirectory);

            var demo = new RijndaelDemo(testDataDirectory, outputDirectory);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1 Encrypt/decrypt byte arrays");
                Console.WriteLine("2 Encrypt/decrypt files");
                Console.WriteLine("0 Exit");
                Console.Write("\n>> ");

                var key = Console.ReadKey().KeyChar;
                Console.WriteLine();

                switch (key)
                {
                    case '1':
                        demo.RunArrayDemo();
                        break;

                    case '2':
                        await demo.RunFileDemoAsync();
                        break;

                    case '0':
                        return;

                    default:
                        Console.WriteLine("???>>");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
